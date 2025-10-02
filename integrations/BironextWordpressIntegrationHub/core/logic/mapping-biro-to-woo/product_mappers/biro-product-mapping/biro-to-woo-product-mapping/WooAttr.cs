namespace BiroWoocommerceHubTests.tools {
    public class WooAttr {

        private string name;
        private bool visible;
        private bool mandatory;
        
        public WooAttr(string name = "", bool visible = true, bool mandatory = false) {
            this.name = name;
            this.visible = visible;
            this.mandatory = mandatory;

            // why the hell was this done?
            //Visible = true;
            //Mandatory = false;
        }

        public string Name { get => name; set => name = value; }
        public bool Visible { get => visible; set => visible = value; }
        public bool Mandatory { get => mandatory; set => mandatory = value; }
    }
}