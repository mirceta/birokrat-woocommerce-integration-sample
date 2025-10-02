using System;
using System.Linq;
using LibGit2Sharp;


namespace common_git
{
    public class CommonGit
    {
        public static void PullGitBranch(
            string repositoryBasePath,
            string repositoryName,
            string username,
            string password,
            string email,
            string branchName) {

            string repositoryPath = $@"{repositoryBasePath}\{repositoryName}";
            string repositoryUrl = $"https://github.com/{username}/{repositoryName}.git";

            // Clone if repository does not exist locally
            if (!Repository.IsValid(repositoryPath))
            {
                Console.WriteLine($"Repository does not exist at '{repositoryPath}', cloning...");

                var cloneOptions = new CloneOptions { BranchName = branchName };

                cloneOptions.FetchOptions.CredentialsProvider = (url, usernameFromUrl, types) =>
                    new UsernamePasswordCredentials { Username = username, Password = password };

                Repository.Clone(repositoryUrl, repositoryPath, cloneOptions);
                Console.WriteLine(
                    $"Cloned repository from '{repositoryUrl}' to '{repositoryPath}'."
                );
            }

            var repo = new Repository(repositoryPath);
            var remote = repo.Network.Remotes["origin"];
            var fetchOptions = new FetchOptions
            {
                CredentialsProvider = (url, usernameFromUrl, types) =>
                    new UsernamePasswordCredentials { Username = username, Password = password },
            };

            // Fetch the specific branch
            var refspec = $"+refs/heads/{branchName}:refs/remotes/origin/{branchName}";
            repo.Network.Fetch(remote.Name, new[] { refspec }, fetchOptions, null);

            // Ensure branch is tracked locally and checked out
            Branch branch = repo.Branches[branchName] ?? repo.Branches[$"origin/{branchName}"];
            if (branch.IsRemote)
            {
                branch = repo.CreateBranch(branchName, branch.Tip);
                repo.Branches.Update(
                    branch,
                    b => b.TrackedBranch = $"refs/remotes/origin/{branchName}"
                );
            }
            else
            {
                repo.Branches.Update(
                    branch,
                    b => b.TrackedBranch = $"refs/remotes/origin/{branchName}"
                );
            }

            Commands.Checkout(repo, branch);
            Console.WriteLine($"Checked out branch '{branchName}'.");

            // Pull the latest changes
            var pullOptions = new PullOptions { FetchOptions = fetchOptions };
            var signature = new Signature(username, email, DateTimeOffset.Now);
            Commands.Pull(repo, signature, pullOptions);
            Console.WriteLine($"Pulled branch '{branchName}'.");
        }

        public static void CreateAndPushTag(
                                string repositoryPath,
                                string tagName,
                                string username,
                                string email,
                                string password) {

            if (!Repository.IsValid(repositoryPath))
            {
                throw new InvalidOperationException($"No valid Git repository found at {repositoryPath}");
            }

            using (var repo = new Repository(repositoryPath))
            {
                var tagger = new Signature(username, email, DateTimeOffset.Now);

                if (repo.Tags[tagName] != null)
                {
                    throw new InvalidOperationException($"Tag '{tagName}' already exists in the repository.");
                }

                Tag tag = repo.Tags.Add(tagName, repo.Head.Tip, tagger, "Auto-created tag");
                var pushOptions = new PushOptions
                {
                    CredentialsProvider = (url, user, types) =>
                        new UsernamePasswordCredentials { Username = username, Password = password }
                };

                repo.Network.Push(repo.Network.Remotes["origin"], $"refs/tags/{tagName}", pushOptions);
            }
        }

        public static bool HasChanges(string repositoryPath) {
            return HasStagedChanges(repositoryPath) || HasUnstagedChanges(repositoryPath);
        }

        private static bool HasStagedChanges(string repositoryPath)
        {
            if (!Repository.IsValid(repositoryPath))
                throw new InvalidOperationException($"Invalid Git repository at {repositoryPath}");

            using (var repo = new Repository(repositoryPath))
            {
                RepositoryStatus status = repo.RetrieveStatus(new StatusOptions());

                // Check if any file is staged (Index) but not yet committed
                return status.Any(entry =>
                    entry.State.HasFlag(FileStatus.NewInIndex) ||
                    entry.State.HasFlag(FileStatus.ModifiedInIndex) ||
                    entry.State.HasFlag(FileStatus.DeletedFromIndex) ||
                    entry.State.HasFlag(FileStatus.RenamedInIndex) ||
                    entry.State.HasFlag(FileStatus.TypeChangeInIndex)
                );
            }
        }

        private static bool HasUnstagedChanges(string repositoryPath)
        {
            if (!Repository.IsValid(repositoryPath))
                throw new InvalidOperationException($"Invalid Git repository at {repositoryPath}");

            using (var repo = new Repository(repositoryPath))
            {
                var status = repo.RetrieveStatus();

                return status.Any(entry =>
                    entry.State.HasFlag(FileStatus.NewInWorkdir) ||
                    entry.State.HasFlag(FileStatus.ModifiedInWorkdir) ||
                    entry.State.HasFlag(FileStatus.DeletedFromWorkdir) ||
                    entry.State.HasFlag(FileStatus.TypeChangeInWorkdir)
                );
            }
        }

        public static string GetCurrentBranchName(string repositoryPath)
        {
            if (!Repository.IsValid(repositoryPath))
            {
                throw new InvalidOperationException($"No valid Git repository found at {repositoryPath}");
            }

            using (var repo = new Repository(repositoryPath))
            {
                return repo.Head.FriendlyName;
            }
        }

        public static bool BranchContainsNewCommits(
            string repositoryPath,
            string fromBranchName, // typically current branch
            string toBranchName    // typically staging
        )
        {
            if (!Repository.IsValid(repositoryPath))
                throw new InvalidOperationException($"Invalid Git repository at {repositoryPath}");

            using (var repo = new Repository(repositoryPath))
            {
                var fromBranch = repo.Branches[fromBranchName];
                var toBranch = repo.Branches[toBranchName];

                if (fromBranch == null || toBranch == null)
                    throw new ArgumentException("One or both branches do not exist.");

                // Commits in 'toBranch' but not in 'fromBranch'
                var newCommits = repo.Commits.QueryBy(new CommitFilter
                {
                    IncludeReachableFrom = toBranch,
                    ExcludeReachableFrom = fromBranch
                });

                return newCommits.Any();
            }
        }

        public static bool IsBranchUpToDateWithRemote(
            string repositoryPath,
            string branchName = "staging",
            string username = null,
            string password = null
        )
        {
            if (!Repository.IsValid(repositoryPath))
                throw new InvalidOperationException($"Invalid Git repository at {repositoryPath}");

            using (var repo = new Repository(repositoryPath))
            {
                var remote = repo.Network.Remotes["origin"];

                // Fetch latest updates from origin
                var fetchOptions = new FetchOptions();

                if (username != null && password != null)
                {
                    fetchOptions.CredentialsProvider = (url, user, cred) =>
                        new UsernamePasswordCredentials { Username = username, Password = password };
                }

                repo.Network.Fetch(remote.Name, new[] { $"refs/heads/{branchName}:refs/remotes/origin/{branchName}" }, fetchOptions, null);

                var localBranch = repo.Branches[branchName];
                var remoteBranch = repo.Branches[$"origin/{branchName}"];

                if (localBranch == null || remoteBranch == null)
                    throw new InvalidOperationException($"Branch '{branchName}' or 'origin/{branchName}' not found.");

                var divergence = repo.ObjectDatabase.CalculateHistoryDivergence(localBranch.Tip, remoteBranch.Tip);

                return divergence.AheadBy == 0 && divergence.BehindBy == 0;
            }
        }

        public static void CommitAndPushAll(
    string repositoryPath,
    string commitMessage,
    string username,
    string email,
    string password,
    string branchName = null)
        {
            if (!Repository.IsValid(repositoryPath))
                throw new InvalidOperationException($"Invalid Git repository at {repositoryPath}");

            using (var repo = new Repository(repositoryPath))
            {
                // Determine target branch
                var targetBranchName = string.IsNullOrWhiteSpace(branchName)
                    ? repo.Head.FriendlyName
                    : branchName;

                var branch = repo.Branches[targetBranchName];
                if (branch == null)
                    throw new InvalidOperationException($"Branch '{targetBranchName}' not found.");

                // Checkout branch if not already on it
                if (repo.Head.FriendlyName != targetBranchName)
                    Commands.Checkout(repo, branch);

                // Stage all changes
                Commands.Stage(repo, "*");

                // If nothing to commit, skip commit but still allow push (in case we’re ahead)
                var status = repo.RetrieveStatus(new StatusOptions());
                bool hasWorkToCommit = status.IsDirty; // any staged/unstaged changes now staged

                // Commit if needed
                if (hasWorkToCommit)
                {
                    var author = new Signature(username, email, DateTimeOffset.Now);
                    var committer = author;
                    repo.Commit(commitMessage, author, committer);
                }

                if (branch.IsRemote)
                    throw new InvalidOperationException($"'{targetBranchName}' refers to a remote branch. Checkout/create a local branch first.");

                // If no upstream, set origin/<branch>
                if (branch.TrackedBranch == null)
                {
                    var remote = repo.Network.Remotes["origin"];
                    if (remote == null)
                        throw new InvalidOperationException("Remote 'origin' not found.");

                    repo.Branches.Update(branch, b => b.TrackedBranch = $"refs/remotes/origin/{targetBranchName}");
                }

                // Push
                var pushOptions = new PushOptions
                {
                    CredentialsProvider = (url, user, types) =>
                        new UsernamePasswordCredentials { Username = username, Password = password }
                };

                // push refspec "refs/heads/<branch>"
                repo.Network.Push(repo.Network.Remotes["origin"], $"refs/heads/{targetBranchName}", pushOptions);
            }
        }
    }
}
