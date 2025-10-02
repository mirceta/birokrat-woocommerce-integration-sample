namespace gui_generator {
    public class EntryObject<T> { // makes the whole mechanism simpler if root is always a fixed class!
        T entry; 
        public EntryObject(T entry) {
            this.entry = entry;
        }
    }
}
