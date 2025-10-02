using System;
using System.Collections.Generic;
using System.Text;

namespace common_google.inbox_state {
    public interface IInboxState {
        void Add(string mailId);
        void AddSuccessful(string mailId);
        void AddFailed(string mailId);
        bool isProcessed(string mailId);
    }
}
