// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using Xunit;

    public sealed class PrimaryKeyNameForeignKeyDiscoveryConventionTests
    {
        [Fact]
        public void Apply_should_discover_simple_matching_foreign_key()
        {
            var associationType = CreateAssociationType();

            var pkProperty = EdmProperty.CreatePrimitive("PId", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
            associationType.SourceEnd.GetEntityType().AddKeyMember(pkProperty);

            var fkProperty = EdmProperty.CreatePrimitive("PId", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
            associationType.TargetEnd.GetEntityType().AddMember(fkProperty);

            // Foo.PId == Bar.PId

            (new PrimaryKeyNameForeignKeyDiscoveryConvention())
                .Apply(associationType, new DbModel(new EdmModel(DataSpace.CSpace), null));

            Assert.NotNull(associationType.Constraint);
            Assert.Same(associationType.TargetEnd, associationType.Constraint.ToRole);
            Assert.Equal("PId", associationType.Constraint.ToProperties.Single().Name);
        }

        [Fact]
        public void Apply_should_discover_simple_matching_foreign_key_with_different_casing()
        {
            var associationType = CreateAssociationType();

            var pkProperty = EdmProperty.CreatePrimitive("PID", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
            associationType.SourceEnd.GetEntityType().AddKeyMember(pkProperty);

            var fkProperty = EdmProperty.CreatePrimitive("PId", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
            associationType.TargetEnd.GetEntityType().AddMember(fkProperty);

            // Foo.PID == Bar.PId

            (new PrimaryKeyNameForeignKeyDiscoveryConvention())
                .Apply(associationType, new DbModel(new EdmModel(DataSpace.CSpace), null));

            Assert.NotNull(associationType.Constraint);
            Assert.Same(associationType.TargetEnd, associationType.Constraint.ToRole);
            Assert.Equal("PId", associationType.Constraint.ToProperties.Single().Name);
        }

        [Fact]
        public void Apply_should_discover_composite_matching_foreign_keys()
        {
            var associationType = CreateAssociationType();

            var pkProperty1 = EdmProperty.CreatePrimitive("PId1", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
            var pkProperty2 = EdmProperty.CreatePrimitive("PId2", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
            associationType.SourceEnd.GetEntityType().AddKeyMember(pkProperty1);
            associationType.SourceEnd.GetEntityType().AddKeyMember(pkProperty2);

            var fkProperty1 = EdmProperty.CreatePrimitive("PId1", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
            var fkProperty2 = EdmProperty.CreatePrimitive("PId2", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
            associationType.TargetEnd.GetEntityType().AddMember(fkProperty1);
            associationType.TargetEnd.GetEntityType().AddMember(fkProperty2);

            // Foo.PId1 == Bar.PId1 && Foo.PId2 == Bar.PId2

            (new PrimaryKeyNameForeignKeyDiscoveryConvention())
                .Apply(associationType, new DbModel(new EdmModel(DataSpace.CSpace), null));

            Assert.NotNull(associationType.Constraint);
            Assert.Same(associationType.TargetEnd, associationType.Constraint.ToRole);
            Assert.Equal(2, associationType.Constraint.ToProperties.Count());
        }

        [Fact]
        public void Apply_should_not_discover_when_multiple_associations_exist()
        {
            var associationType = CreateAssociationType();

            var pkProperty = EdmProperty.CreatePrimitive("PId", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
            associationType.SourceEnd.GetEntityType().AddKeyMember(pkProperty);

            var fkProperty = EdmProperty.CreatePrimitive("PId", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
            associationType.TargetEnd.GetEntityType().AddMember(fkProperty);

            // Foo.PId == Bar.PId

            var model = new EdmModel(DataSpace.CSpace);
            model.AddItem(associationType);
            model.AddItem(associationType);

            (new PrimaryKeyNameForeignKeyDiscoveryConvention())
                .Apply(associationType, new DbModel(model, null));

            Assert.Null(associationType.Constraint);
        }

        [Fact]
        public void Apply_should_not_discover_when_property_types_are_incompatible()
        {
            var associationType = CreateAssociationType();

            var pkProperty = EdmProperty.CreatePrimitive("PId", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Binary));
            associationType.SourceEnd.GetEntityType().AddKeyMember(pkProperty);

            var fkProperty = EdmProperty.CreatePrimitive("PId", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
            associationType.TargetEnd.GetEntityType().AddMember(fkProperty);

            // Foo.PId == Bar.PId

            (new PrimaryKeyNameForeignKeyDiscoveryConvention())
                .Apply(associationType, new DbModel(new EdmModel(DataSpace.CSpace), null));

            Assert.Null(associationType.Constraint);
        }

        private static AssociationType CreateAssociationType()
        {
            var associationType = new AssociationType("A", XmlConstants.ModelNamespace_3, false, DataSpace.CSpace);
            associationType.SourceEnd = new AssociationEndMember("S", new EntityType("E", "N", DataSpace.CSpace));
            associationType.TargetEnd = new AssociationEndMember("T", new EntityType("E", "N", DataSpace.CSpace));

            associationType.SourceEnd.RelationshipMultiplicity = RelationshipMultiplicity.ZeroOrOne;

            associationType.TargetEnd.RelationshipMultiplicity = RelationshipMultiplicity.Many;

            return associationType;
        }
    }
}
