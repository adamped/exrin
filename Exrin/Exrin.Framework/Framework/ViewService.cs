﻿namespace Exrin.Framework
{
    using Abstraction;
    using Common;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    public class ViewService : IViewService
    {
        private readonly IInjectionProxy _injection = null;
        private IDictionary<Type, IView> _cachedViews = null;
        private object _lock = new object();

        public ViewService(IInjectionProxy injection)
        {
            _injection = injection;
        }

        private readonly IDictionary<Type, TypeDefinition> _viewsByType = new ConcurrentDictionary<Type, TypeDefinition>();

        private object GetBindingContext(Type viewType)
        {
            var viewModelType = _viewsByType[viewType];

            ConstructorInfo constructor = null;

            var parameters = new object[] { };

            constructor = viewModelType.Type.GetTypeInfo()
                   .DeclaredConstructors
                   .FirstOrDefault(c => !c.GetParameters().Any());

            if (constructor == null)
            {
                constructor = viewModelType.Type.GetTypeInfo()
                   .DeclaredConstructors.First();

                var parameterList = new List<object>();

                foreach (var param in constructor.GetParameters())
                    parameterList.Add(_injection.Get(param.ParameterType));

                parameters = parameterList.ToArray();
            }

            if (constructor == null)
                throw new InvalidOperationException(
                    $"No suitable constructor found for ViewModel {viewModelType.ToString()}");

            var context = constructor.Invoke(parameters);

			// ICompositional
			var container = _injection.Get(typeof(IExrinContainer)) as IExrinContainer;

			(context as IComposition).SetContainer(container);

			return context;
        }

        public Task<IView> Build(ITypeDefinition definition)
        {
            lock (_lock)
            {
                IView view = null;

                if (definition.CacheView && _cachedViews.ContainsKey(definition.Type))
                    view = _cachedViews[definition.Type];
                else
                {
                    ConstructorInfo constructor = null;
                    object[] parameters = null;

                    constructor = definition.Type.GetTypeInfo()
                        .DeclaredConstructors
                        .FirstOrDefault(c => !c.GetParameters().Any());

                    parameters = new object[] { };

                    if (constructor == null)
                        throw new InvalidOperationException(
                            $"No suitable constructor found for view {definition.Type.ToString()}");

                    ThreadHelper.RunOnUIThread(() =>
                    {
                        view = constructor.Invoke(parameters) as IView;
                    });

                    if (view == null)
                        throw new InvalidOperationException(
                            $"View {definition.Type.ToString()} does not implement the interface {nameof(IView)}");

                    // Assign Binding Context
                    if (_viewsByType.ContainsKey(definition.Type))
                    {
                        view.BindingContext = GetBindingContext(definition.Type);

                        var multiView = view as IMultiView;

                        if (multiView != null)
                            foreach (var p in multiView.Views)
                                p.BindingContext = GetBindingContext(p.GetType());
                    }
                    else
                        throw new InvalidOperationException(
                            "No suitable view model found for view " + definition.Type.ToString());

                    if (definition.CacheView)
                        _cachedViews.Add(definition.Type, view);
                }

                return Task.FromResult(view);
            }
        }

        public void Map(Type viewType, Type viewModelType)
        {
            lock (_viewsByType)
                if (_viewsByType.ContainsKey(viewType))
                    _viewsByType[viewType] = new TypeDefinition() { Type = viewModelType };
                else
                    _viewsByType.Add(viewType, new TypeDefinition() { Type = viewModelType });
        }

        public Type GetMap(Type viewModelType)
        {
            foreach (var type in _viewsByType)
                if (type.Value.Type == viewModelType)
                    return type.Key;

            return null;
        }
    }
}
