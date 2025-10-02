using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Upload;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace common_google.drive
{
    public class DriveOperator
    {

        public static string[] Scopes = { DriveService.Scope.Drive, DriveService.Scope.DriveFile };
        DriveService service;

        public DriveOperator(UserCredential cred) {
            service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = cred,
                ApplicationName = "birokrat-next-mobile"
            });
        }


        public string FindIdOfFilename(string filename, string parentFolderId = null) {
            FilesResource.ListRequest listRequest = service.Files.List();
            listRequest.PageSize = 10;
            listRequest.Fields = "nextPageToken, files(id, name)";
            listRequest.Q = $"name = '{filename}'";
            if (parentFolderId != null) {
                listRequest.Q += $" and '{parentFolderId}' in parents";
            }


            // List files.
            IList<Google.Apis.Drive.v3.Data.File> files = listRequest.Execute()
                .Files;
            Console.WriteLine("Files:");
            if (files != null && files.Count > 0)
            {
                foreach (var file in files)
                {
                    if (file.Name == filename) {
                        return file.Id;
                    }
                }
                return null;
            }
            else
            {
                Console.WriteLine("No files found.");
            }
            return null;
        }

        public string FindIdOfFilenameByPath(string filepath) {
            string[] path_sep = filepath.Split(new char[] { '/', '\\' });
            string id = null;
            for (int i = 0; i < path_sep.Length; i++)
            {
                string new_id = FindIdOfFilename(path_sep[i], id);
                if (new_id == null)
                {
                    return null;
                }
                else
                {
                    id = new_id;
                }
            } 
            return id;
        }

        public string GetOrCreatePath(string path)
        {
            // gets the innermost folder in the path and creates the folder hierarchy
            // if any level is missing
            string[] path_sep = path.Split(new char[] { '/', '\\' });
            string id = null;
            for (int i = 0; i < path_sep.Length; i++)
            {
                string new_id = FindIdOfFilename(path_sep[i], id);
                if (new_id == null)
                {
                    id = CreateFolderRetId(path_sep[i], id);
                }
                else {
                    id = new_id;
                }
            }

            return id;
        }

        // list directory
        public void ListDirectory(string root_id) {
            // Define parameters of request.
            FilesResource.ListRequest listRequest = service.Files.List();
            listRequest.PageSize = 10;
            listRequest.Fields = "nextPageToken, files(id, name)";

            // List files.
            IList<Google.Apis.Drive.v3.Data.File> files = listRequest.Execute()
                .Files;
            Console.WriteLine("Files:");
            if (files != null && files.Count > 0)
            {
                foreach (var file in files)
                {
                    Console.WriteLine("{0} ({1})", file.Name, file.Id);
                }
            }
            else
            {
                Console.WriteLine("No files found.");
            }
        }

        public string CreateFolderRetId(string folder_name, string parentId = null) {

            var metadata = new Google.Apis.Drive.v3.Data.File() {
                Name = folder_name,
                MimeType = "application/vnd.google-apps.folder"
            };
            if (parentId != null)
                metadata.Parents = new string[] { parentId }.ToList();

            var req = service.Files.Create(metadata);
            req.Fields = "id";
            Google.Apis.Drive.v3.Data.File res = req.Execute();
            return res.Id;
        }

        public void UploadByDirectoryPath(string local_path, string remote_path)
        {
            string directory_id = "";
            if (remote_path.Contains("."))
                directory_id = GetOrCreatePath(Path.GetDirectoryName(remote_path));
            else
                directory_id = GetOrCreatePath(remote_path);

            string desired_name = Path.GetFileName(remote_path);

            UploadFile(local_path, desired_name, directory_id);
        }

        // upload
        public Google.Apis.Drive.v3.Data.File UploadFile(string _uploadFile, string new_name, string _parentId)
        {

            if (System.IO.File.Exists(_uploadFile))
            {
                var path = _uploadFile;
                var filename = Path.GetFileName(path);
                var mimetype = "application/octet-stream";

                var metadata = new Google.Apis.Drive.v3.Data.File() { Name = filename };
                if (new_name != null)
                    metadata.Name = new_name;

                // find out of file exists
                string id = FindIdOfFilename(metadata.Name, _parentId);
                if (id != null)
                {
                    SendUpdateRequest(path, mimetype, metadata, id);
                }
                else
                {
                    SendCreateRequest(_parentId, path, mimetype, metadata);
                }
            }
            else
            {
                throw new Exception("File not found");
            }
            return null;
        }


        #region [auxiliary]
        private void SendCreateRequest(string _parentId, string path, string mimetype, Google.Apis.Drive.v3.Data.File metadata)
        {
            if (_parentId != null)
                metadata.Parents = new string[] { _parentId }.ToList();
            FilesResource.CreateMediaUpload request;
            using (var stream = new System.IO.FileStream(path, FileMode.Open))
            {
                request = service.Files.Create(
                    metadata, stream, mimetype);
                request.Fields = "id";
                IUploadProgress u = request.Upload();
                Exception ex = u.Exception;
                UploadStatus s = u.Status;
                if (ex != null)
                    throw ex;
            }
            var response = request.ResponseBody;
        }

        private void SendUpdateRequest(string path, string mimetype, Google.Apis.Drive.v3.Data.File metadata, string id)
        {
            FilesResource.UpdateMediaUpload request;
            using (var stream = new System.IO.FileStream(path, FileMode.Open))
            {
                request = service.Files.Update(
                    metadata, id, stream, mimetype);
                request.Fields = "id";
                IUploadProgress u = request.Upload();
                Exception ex = u.Exception;
                UploadStatus s = u.Status;
                if (ex != null)
                    throw ex;
            }
            var response = request.ResponseBody;
        }
        #endregion

    }
}
