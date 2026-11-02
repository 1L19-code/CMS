namespace CMS.Editor
{
    public abstract class ExtensionInstaller
    {
        /// <summary>
        /// Use it to cast object registrator to generic IRegistrator
        /// </summary>
        /// <param name="locator"></param>
        /// <param name="registrator"></param>
        internal abstract bool TryInstall(Locator locator, IRegistrationResolver registrator);
    }

    public abstract class EditorExtensionInstaller<TKey, TInstance> : ExtensionInstaller
    {
        internal override bool TryInstall(Locator locator, IRegistrationResolver registrator)
        {
            if (registrator is not IRegistrator<TKey, TInstance> casted)
                return false;

            Install(locator, casted);
            return true;
        }

        public abstract void Install(Locator locator, IRegistrator<TKey, TInstance> registrator);
    }

    public abstract class EditorExtensionInstaller<TInstance> : ExtensionInstaller
    {
        internal override bool TryInstall(Locator locator, IRegistrationResolver registrator)
        {
            if (registrator is not IRegistrator<TInstance> casted)
                return false;

            Install(locator, casted);
            return true;
        }

        public abstract void Install(Locator locator, IRegistrator<TInstance> registrator);
    }
}