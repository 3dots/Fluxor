using System;
using System.Threading.Tasks;

namespace Fluxor.DependencyInjection.Wrappers
{
	internal class EffectWrapper<TAction> : IEffect
	{
		private delegate Task HandleWithActionParameterAsyncHandler(TAction action, IDispatcher dispatcher);
		private delegate void HandleWithActionParameterVoidHandler(TAction action, IDispatcher dispatcher);

		private delegate Task HandleWithActonParameterNoDispatcherAsyncHandler(TAction action);
		private delegate void HandleWithActionParameterNoDispatcherVoidHandler(TAction action);

		private delegate Task HandleWithoutActionParameterAsyncHandler(IDispatcher dispatcher);
		private delegate void HandleWithoutActionParameterVoidHandler(IDispatcher dispatcher);

		private readonly HandleWithActionParameterAsyncHandler HandleAsync;

		Task IEffect.HandleAsync(object action, IDispatcher dispatcher) => HandleAsync((TAction)action, dispatcher);
		bool IEffect.ShouldReactToAction(object action) => action is TAction;

		public EffectWrapper(object effectHostInstance, EffectMethodInfo effectMethodInfo)
		{
			HandleWithActionParameterAsyncHandler handler;

			if (effectMethodInfo.RequiresActionParameterInMethod)
			{
				if (effectMethodInfo.RequiresDispatcherParameterInMethod)
				{
					if (effectMethodInfo.IsVoid)
						handler = WrapVoidEffectWithActionAndDispatcherParameter(effectHostInstance, effectMethodInfo);
					else
						handler = CreateDelegate<HandleWithActionParameterAsyncHandler>(effectHostInstance, effectMethodInfo);
				}
				else
				{
					if (effectMethodInfo.IsVoid)
						handler = WrapVoidEffectWithActionParameter(effectHostInstance, effectMethodInfo);
					else
						handler = WrapAsyncEffectWithActionParameter(effectHostInstance, effectMethodInfo);
				}
			}
			else
			{
				if (effectMethodInfo.IsVoid)
					handler = WrapVoidEffectWithDispatcherParameter(effectHostInstance, effectMethodInfo);
				else
					handler = WrapAsyncEffectWithDispatcherParameter(effectHostInstance, effectMethodInfo);
			}

			HandleAsync = handler;
		}

		private static HandleWithActionParameterAsyncHandler WrapVoidEffectWithActionAndDispatcherParameter(
			object effectHostInstance,
			EffectMethodInfo effectMethodInfo)
		{
			var handler = CreateDelegate<HandleWithActionParameterVoidHandler>(effectHostInstance, effectMethodInfo);

			return new HandleWithActionParameterAsyncHandler((action, dispatcher) =>
			{
				handler.Invoke(action, dispatcher);
				return Task.CompletedTask;
			});
		}

		private static HandleWithActionParameterAsyncHandler WrapVoidEffectWithActionParameter(
			object effectHostInstance,
			EffectMethodInfo effectMethodInfo)
		{
			var handler = CreateDelegate<HandleWithActionParameterNoDispatcherVoidHandler>(effectHostInstance, effectMethodInfo);

			return new HandleWithActionParameterAsyncHandler((action, dispatcher) =>
			{
				handler.Invoke(action);
				return Task.CompletedTask;
			});
		}

		private static HandleWithActionParameterAsyncHandler WrapVoidEffectWithDispatcherParameter(
			object effectHostInstance,
			EffectMethodInfo effectMethodInfo)
		{
			var handler = CreateDelegate<HandleWithoutActionParameterVoidHandler>(effectHostInstance, effectMethodInfo);

			return new HandleWithActionParameterAsyncHandler((action, dispatcher) =>
			{
				handler.Invoke(dispatcher);
				return Task.CompletedTask;
			});
		}
		
		private static HandleWithActionParameterAsyncHandler WrapAsyncEffectWithActionParameter(
			object effectHostInstance,
			EffectMethodInfo effectMethodInfo)
		{
			var handler = CreateDelegate<HandleWithActonParameterNoDispatcherAsyncHandler>(effectHostInstance, effectMethodInfo);
			return new HandleWithActionParameterAsyncHandler((action, dispatcher) => handler.Invoke(action));
		}

		private static HandleWithActionParameterAsyncHandler WrapAsyncEffectWithDispatcherParameter(
			object effectHostInstance,
			EffectMethodInfo effectMethodInfo)
		{
			var handler = CreateDelegate<HandleWithoutActionParameterAsyncHandler>(effectHostInstance, effectMethodInfo);
			return new HandleWithActionParameterAsyncHandler((action, dispatcher) => handler.Invoke(dispatcher));
		}

		private static DelegateType CreateDelegate<DelegateType>(
			object effectHostInstance, 
			EffectMethodInfo effectMethodInfo) where DelegateType : Delegate
			=> (DelegateType)Delegate.CreateDelegate(
						type: typeof(DelegateType),
						firstArgument: effectHostInstance,
						method: effectMethodInfo.MethodInfo);
	}
}
