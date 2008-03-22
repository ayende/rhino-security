namespace Rhino.Security
{
    using System;
    using Castle.ActiveRecord.Framework.Internal;

    internal class ReplaceUserVisitor : AbstractDepthFirstVisitor
    {
        private readonly Type userType;

        public ReplaceUserVisitor(Type userType)
        {
            this.userType = userType;
        }

        public override void VisitBelongsTo(BelongsToModel model)
        {
            if (model.Property.PropertyType == typeof(IUser))
                model.BelongsToAtt.Type = userType;
        }

        public override void VisitHasAndBelongsToMany(HasAndBelongsToManyModel model)
        {
            if (model.HasManyAtt.MapType == typeof(IUser))
            {
                model.HasManyAtt.MapType = userType;
            }
        }

        public override void VisitHasMany(HasManyModel model)
        {
            if (model.HasManyAtt.MapType == typeof(IUser))
            {
                model.HasManyAtt.MapType = userType;
            }
        }
    }
}