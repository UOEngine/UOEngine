namespace UOEngine
{
    public interface IUOEngineApp
    {
        // Run anything that does not depend on native.
        public abstract bool PreEngineInit();

        // Native engine has initialised at this point.
        public abstract bool Initialise();

        // Called every frame post native input, pre native render.
        public abstract void Update(float deltaTime);

    }
}
