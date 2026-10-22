namespace CMS.Editor
{
    public abstract class ExtensionInstaller
    {
        /// <summary>
        /// Use it to cast object registrator to generic IRegistrator
        /// </summary>
        /// <param name="registrator"></param>
        internal abstract bool TryInstall(IRegistrationResolver registrator);
    }

    public abstract class ExtensionInstaller<TKey, TInstance> : ExtensionInstaller
    {
        internal override bool TryInstall(IRegistrationResolver registrator)
        {
            if (registrator is not IRegistrator<TKey, TInstance> casted)
                return false;

            Install(casted);
            return true;
        }

        public abstract void Install(IRegistrator<TKey, TInstance> registrator);
    }

    public abstract class ExtensionInstaller<TInstance> : ExtensionInstaller
    {
        internal override bool TryInstall(IRegistrationResolver registrator)
        {
            if (registrator is not IRegistrator<TInstance> casted)
                return false;

            Install(casted);
            return true;
        }
        
        public abstract void Install(IRegistrator<TInstance> registrator);
    }
}
