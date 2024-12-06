using System;
using System.Runtime.Serialization;


namespace ADO.BL.Responses
{
    [Serializable]
    [DataContract]
    public class ResponseEntity<T>
    {
        [DataMember]
        public bool Success { get; set; }
        [DataMember]
        public bool SuccessData { get; set; }
        [DataMember]
        public T Data { get; set; }
        [DataMember]
        public string Message { get; set; }
    }
}
