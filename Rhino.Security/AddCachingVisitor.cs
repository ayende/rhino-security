namespace Rhino.Security
{
    using Castle.ActiveRecord;
    using Castle.ActiveRecord.Framework.Internal;

    internal class AddCachingVisitor : AbstractDepthFirstVisitor
    {
        public override void VisitHasMany(HasManyModel model)
        {
            model.HasManyAtt.Cache = CacheEnum.ReadWrite;
        }

        public override void VisitHasAndBelongsToMany(HasAndBelongsToManyModel model)
        {
            model.HasManyAtt.Cache = CacheEnum.ReadWrite;
        }

        public override void VisitModel(ActiveRecordModel model)
        {
            model.ActiveRecordAtt.Cache = CacheEnum.ReadWrite;
            base.VisitModel(model);
        }
    }
}