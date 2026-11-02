namespace CMS
{
    public abstract class RuntimeExtensionInstaller
    {
        internal abstract bool TryInstall(Locator locator, IRegistrationResolver registrator);
    }

    public abstract class RuntimeExtensionInstaller<TKey, TInstance> : RuntimeExtensionInstaller
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

    public abstract class RuntimeExtensionInstaller<TInstance> : RuntimeExtensionInstaller
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
