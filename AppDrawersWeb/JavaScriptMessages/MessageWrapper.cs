using Newtonsoft.Json;

namespace AppDrawers.JavaScriptMessages
{
    public class MessageWrapper
    {
        public object Data { get; set; } = null;
        public string Type { get; set; } = null;

        public static MessageWrapper FromJson(string json)
        {
            return JsonConvert.DeserializeObject<MessageWrapper>(json);
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}