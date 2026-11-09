namespace CMS
{
    /// <summary>Creates independent runtime content services.</summary>
    public static class ContentFactory
    {
        /// <summary>
        /// Creates an uninitialized content service. Call and await
        /// <see cref="IContent.IndexAsync"/> on Unity's main thread before retrieving content.
        /// </summary>
        public static IContent Create()
        {
            return new ContentManager();
        }
    }
}
