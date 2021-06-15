#nullable enable
using System.Collections.Generic;

namespace SKD.Service {

    public class MutationPayload<T> where T : class {
        public MutationPayload(T? payload) {
            Payload = payload;
        }
        public T? Payload { get; set; }
        public List<Error> Errors { get; set; } = new List<Error>();
    }
}