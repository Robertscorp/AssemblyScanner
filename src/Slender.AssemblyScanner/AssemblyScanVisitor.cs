using System;
using System.Collections.Generic;
using System.Linq;

namespace Slender.AssemblyScanner
{

    public abstract class AssemblyScanVisitor
    {

        #region - - - - - - Methods - - - - - -

        private static IEnumerable<Type> GetAbstractBases(Type type)
            => type == null
                ? Enumerable.Empty<Type>()
                : type.IsAbstract
                    ? GetAbstractBases(type.BaseType).Union(new[] { type })
                    : GetAbstractBases(type.BaseType);

        private static List<Type> GetOrAdd(Dictionary<Type, List<Type>> dictionary, Type type)
        {
            if (!dictionary.TryGetValue(type, out var _List))
            {
                _List = new List<Type>();
                dictionary.Add(type, _List);
            }
            return _List;
        }

        /// <summary>
        /// Visits the abstract Type.
        /// </summary>
        /// <param name="abstractType">The abstract Type being visited.</param>
        protected virtual void VisitAbstract(Type abstractType) { }

        /// <summary>
        /// Visits the Abstract Type and its inheritor Types.
        /// </summary>
        /// <param name="abstractType">The abstract Type being visited.</param>
        /// <param name="implementationTypes">The instantiable class Types that inherit the abstract Type.</param>
        protected virtual void VisitAbstractAndImplementations(Type abstractType, IEnumerable<Type> implementationTypes) { }

        /// <summary>
        /// Visits the AssemblyScan.
        /// </summary>
        /// <param name="scan">The AssemblyScan being visited.</param>
        /// <exception cref="ArgumentNullException">Thrown when scan is null.</exception>
        public virtual void VisitAssemblyScan(IAssemblyScan scan)
        {
            if (scan is null) throw new ArgumentNullException(nameof(scan));

            var _AbstractImplementations = new Dictionary<Type, List<Type>>();
            var _InterfaceImplementations = new Dictionary<Type, List<Type>>();

            foreach (var _Type in scan.Types)
            {
                this.VisitType(_Type);

                if (_Type.IsClass && !_Type.IsAbstract && _Type.BaseType != typeof(MulticastDelegate))
                {
                    foreach (var _AbstractBase in GetAbstractBases(_Type))
                        GetOrAdd(_AbstractImplementations, _AbstractBase).Add(_Type);

                    foreach (var _Interface in _Type.GetInterfaces())
                        GetOrAdd(_InterfaceImplementations, _Interface).Add(_Type);
                }
            }

            foreach (var _AbstractAndImplementations in _AbstractImplementations)
                this.VisitAbstractAndImplementations(_AbstractAndImplementations.Key, _AbstractAndImplementations.Value);

            foreach (var _IntercaceAndImplementations in _InterfaceImplementations)
                this.VisitInterfaceAndImplementations(_IntercaceAndImplementations.Key, _IntercaceAndImplementations.Value);
        }

        /// <summary>
        /// Visits the delegate Type.
        /// </summary>
        /// <param name="delegateType">The delegate Type being visited.</param>
        protected virtual void VisitDelegate(Type delegateType) { }

        /// <summary>
        /// Visits the enumeration Type.
        /// </summary>
        /// <param name="enumerationType">The enumeration Type being visited.</param>
        protected virtual void VisitEnumeration(Type enumerationType) { }

        /// <summary>
        /// Visits the instantiable class Type.
        /// </summary>
        /// <param name="implementationType">The instantiable class Type being visited.</param>
        protected virtual void VisitImplementation(Type implementationType) { }

        /// <summary>
        /// Visits the interface Type.
        /// </summary>
        /// <param name="interfaceType">The interface Type being visited.</param>
        protected virtual void VisitInterface(Type interfaceType) { }

        /// <summary>
        /// Visits the interface Type and its implementer Types.
        /// </summary>
        /// <param name="interfaceType">The interface Type being visited.</param>
        /// <param name="implementationTypes">The instantiable class Types that implement the interface Type.</param>
        protected virtual void VisitInterfaceAndImplementations(Type interfaceType, IEnumerable<Type> implementationTypes) { }

        /// <summary>
        /// Visits the Type.
        /// </summary>
        /// <param name="type">The Type being visited.</param>
        protected virtual void VisitType(Type type)
        {
            if (type.IsEnum)
                this.VisitEnumeration(type);

            else if (type.IsInterface) // Needs to be before IsAbstract, as an Interface is Abstract.
                this.VisitInterface(type);

            else if (type.IsAbstract)
            {
                // A Static class is an Abstract Sealed Class.
                if (!type.IsSealed) this.VisitAbstract(type);
            }
            else if (type.IsValueType)
                this.VisitValueType(type);

            else if (type.BaseType == typeof(MulticastDelegate)) // Needs to be before IsClass, as a delegate is a Class.
                this.VisitDelegate(type);

            else if (type.IsClass)
                this.VisitImplementation(type);
        }

        /// <summary>
        /// Visits the value Type.
        /// </summary>
        /// <param name="valueType">The value Type being visited.</param>
        protected virtual void VisitValueType(Type valueType) { }

        #endregion Methods

    }

}
