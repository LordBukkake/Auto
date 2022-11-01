using Auto.Data;
using Auto.Data.Entities;
using Auto.Website.GraphQL.GraphTypes;
using GraphQL;
using GraphQL.Types;
using System.Collections;
using System.Collections.Generic;

namespace Auto.Website.GraphQL.Queries
{
    public class OwnerQuery : ObjectGraphType
    {
        private IAutoDatabase _db;

        public OwnerQuery(IAutoDatabase db)
        {
            _db = db;
            Field<ListGraphType<OwnerGraphType>>("owners")
            .Resolve(GetOwner);
            Field<OwnerGraphType>("ownerEmail")
                .Argument<string>("email")
                .Resolve(OwnerEmail);
        }

        private IEnumerable<Owner> GetOwner(IResolveFieldContext<object> context)
        {
            return _db.ListOwners();
        }

        private Owner OwnerEmail(IResolveFieldContext<object> context)
        {
            var email = context.GetArgument<string>("email");
            return _db.FindOwner(email);
        }
    }
}