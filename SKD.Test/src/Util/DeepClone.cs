using Newtonsoft.Json;

namespace SKD.Test {
    public static class ExtensionMethods {
        public static T DeepCopy<T>(this T self) {
            var serialized = JsonConvert.SerializeObject(self);
            return JsonConvert.DeserializeObject<T>(serialized);
        }
    }
}