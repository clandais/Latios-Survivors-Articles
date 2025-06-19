namespace Survivors.Bootstrap.RootSystems
{
    // [UpdateBefore(typeof(AnnaSuperSystem))]
    // public partial class PreAnnaRootSystem : RootSuperSystem
    // {
    //     EntityQuery m_shouldUpdateQuery;
    //
    //     protected override void CreateSystems()
    //     {
    //         m_shouldUpdateQuery = Fluent.With<PauseRequestedTag>()
    //             .Build();
    //
    //         GetOrCreateAndAddUnmanagedSystem<PreAnnaPauseSystem>();
    //     }
    //
    //     public override bool ShouldUpdateSystem()
    //     {
    //         return !m_shouldUpdateQuery.IsEmptyIgnoreFilter;
    //     }
    // }
}