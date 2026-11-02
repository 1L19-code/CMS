using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace CMS
{
    public class PrefabRuntimeInstaller : RuntimeExtensionInstaller<ReaderId, ICmsPlugin>
    {
        public override void Install(Locator locator, IRegistrator<ReaderId, ICmsPlugin> registrator)
        {
            var container = Resources.Load<PrefabsContainer>("prefabs");
            var prefabs = container == null
                ? new Dictionary<Guid, UnityEngine.Object>()
                : container.Prefabs.ToDictionary
                (
                    x => Guid.Parse(x.Guid),
                    x => x.Prefab
                );

            registrator.Register(PrefabConstants.ReaderId, new PrefabPlugin(prefabs));
        }
    }
}
