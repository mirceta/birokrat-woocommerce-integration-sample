using System;
using System.Collections.Generic;
using System.Text;

namespace core.structs {
    public class PhpPluginConfig {
        public List<string> AcceptableAttachmentOrderStatuses { get; set; }
        public List<string> OrderStatusHooks { get; set; }
        public bool AttachmentHook { get; set; }
        public bool ProductHooks { get; set; }

        public static PhpPluginConfig NullObject() {
            return new PhpPluginConfig()
            {
                ProductHooks = false,
                AcceptableAttachmentOrderStatuses = null,
                AttachmentHook = false,
                OrderStatusHooks = new List<string>() { }
            };
        }
    }
}
