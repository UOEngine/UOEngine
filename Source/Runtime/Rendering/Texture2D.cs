namespace UOEngine.Runtime.Rendering

{
    public class Texture2D
    {
        public Texture2D(uint width, uint height) 
        {
            this.width = width;
            this.height = height;
        }

        //private ImageView imageView;
        //private Image image;
        private uint width;
        private uint height;
    }
}
