using Celeste.Mod.BossesHelper.Code.Entities;
using Monocle;

namespace Celeste.Mod.BossesHelper.Code.Components
{
    [Tracked(false)]
    public class SavePointTracker(GlobalSavePoint savePoint) : Component(active: true, visible: false)
    {
        public GlobalSavePoint savePoint = savePoint;
    }
}
