using gui_generator;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace gui_gen
{
    public class Dememoizer : IDememoizer
    {
        private readonly List<CurrentValue> memos; // Assume this is initialized elsewhere

        public Dememoizer(List<CurrentValue> memos)
        {
            this.memos = memos;
        }

        public CurrentValue Find(string searchedInput) {

            string searched = searchedInput.Replace("@#@#", "");
            var tmp = memos.Where(x => x.type.Contains($"@#@#{searched}")).ToList();

            if (tmp.Count == 1)
            {
                return tmp.Single();
            }
            else if (tmp.Count() > 1)
            {
                string maybeinterface = searched;
                var implementations = memos.Where(x =>
                {
                    return isMemoImplementationOfInterface(x, $"@#@#{maybeinterface}");
                }).ToList();
                if (tmp.Count > 0)
                {
                    var retval = implementations[0];
                    retval.implementationOptions = implementations.ToArray();
                    return retval;
                }

                // case of not interface but still many memos
                throw new System.Exception($"Multiple memos contained in the memos array for type {searchedInput}. This should never happen!");
            }
            else {
                throw new System.Exception($"No memos contained in the memos array for type {searchedInput}. This should never happen!");
            }
        }

        private static bool isMemoImplementationOfInterface(CurrentValue memo, string maybeInterface)
        {
            if (memo.type.Contains("."))
            {
                if (maybeInterface == memo.type.Split('.')[0]) return true;
            }
            return false;
        }

        public CurrentValue Expand(CurrentValue currentValue)
        {
            if (!IsDememoableType(currentValue)) return currentValue;

            currentValue = Explode(currentValue);
            if (currentValue.dependencies != null)
            {
                currentValue.dependencies = currentValue.dependencies.Select(Explode).ToArray();
            }

            return currentValue;
        }

        public CurrentValue Explode(CurrentValue currentValue)
        {
            if (!IsDememoableType(currentValue)) return currentValue;

            currentValue = DememoDep(currentValue);
            if (currentValue.implementationOptions != null)
            {
                currentValue.implementationOptions = currentValue.implementationOptions.Select(DememoImplOpt).ToArray();
            }

            return currentValue;
        }

        // The following methods would be the C# adaptation of the auxiliary functions
        private CurrentValue DememoDep(CurrentValue currentValue)
        {
            if (!currentValue.type.Contains("@#@#")) return currentValue;

            if (DoesDepHaveMultipleImplOpts(currentValue))
            {
                var options = GetOptions(currentValue);
                var firstOption = options.First();
                currentValue.type = firstOption.type;
                currentValue.dependencies = DeepCopy(firstOption.dependencies);
                currentValue = AddMemoedImplOptsForConcreteImpl(currentValue);
                currentValue.type = currentValue.type.Replace("@#@#", "");
            }
            else
            {
                // if list and type == "Object" then cannot add! Because it is a part of dictionary!

                // if list, type == "ConditionOperationPair"
                // in memos find @#@#ConditionOperationPair -> Expand / Explode it!


                // if dictionary, type = "String,String" => ...
                // find and explode both types

                currentValue = DememoConcreteType(currentValue);
            }

            return currentValue;

        }

        private CurrentValue DememoImplOpt(CurrentValue currentValue)
        {
            if (!currentValue.type.Contains("@#@#"))
            {
                currentValue.type = "@#@#" + currentValue.type;
            }

            currentValue = DememoConcreteType(currentValue);

            if (!currentValue.type.Contains("@#@#"))
                currentValue.type = "@#@#" + currentValue.type;

            currentValue = AddMemoedImplOptsForConcreteImpl(currentValue);

            if (currentValue.implementationOptions != null)
                foreach (var option in currentValue.implementationOptions)
                    option.type = option.type.Replace("@#@#", "");

            if (currentValue.type.Contains("@#@#"))
                currentValue.type = currentValue.type.Replace("@#@#", "");

            return currentValue;
        }

        private CurrentValue AddMemoedImplOptsForConcreteImpl(CurrentValue currentValue)
        {
            var typePrefix = currentValue.type.Split('.')[0];
            var options = memos
                .Where(x => x.type.StartsWith(typePrefix))
                .Select(x => DeepCopy(x))
                .ToList();

            if (options.Count > 1) // It's considered an interface with multiple implementations
            {
                var deepCopiedOptions = options.Select(x => DeepCopy(x)).ToList();
                foreach (var option in options)
                {
                    option.implementationOptions = deepCopiedOptions.ToArray();
                }
                currentValue.implementationOptions = options.ToArray();
            }

            return currentValue;
        }

        private CurrentValue DememoConcreteType(CurrentValue currentValue)
        {
            var memo = memos.FirstOrDefault(x => x.type == currentValue.type);
            if (memo != null)
            {
                currentValue.dependencies = DeepCopy(memo.dependencies);
                currentValue.type = memo.type.Replace("@#@#", "");
            }
            return currentValue;
        }


        private bool IsDememoableType(CurrentValue currentValue)
        {
            if (currentValue.value != null && currentValue.value.StartsWith("@@"))
                return false; // these are cached variables - we don't dememo them
            if (currentValue.type == "EntryObject`1")
                return false; // we don't dememo the entry object
            return true;
        }

        private bool DoesDepHaveMultipleImplOpts(CurrentValue currentValue)
        {
            var options = GetOptions(currentValue);
            return !currentValue.type.Contains('.') && options.Count > 1;
        }

        private List<CurrentValue> GetOptions(CurrentValue currentValue)
        {
            return memos
                .Where(x => x.type.StartsWith(currentValue.type))
                .Select(x => DeepCopy(x))
                .ToList();
        }

        private CurrentValue DeepCopy(CurrentValue obj)
        {
            return JsonConvert.DeserializeObject<CurrentValue>(JsonConvert.SerializeObject(obj));
        }
        private CurrentValue[] DeepCopy(CurrentValue[] obj)
        {
            return JsonConvert.DeserializeObject<CurrentValue[]>(JsonConvert.SerializeObject(obj));
        }
    }
}
