using System;
using System.Threading.Tasks;

namespace Fluxor.DependencyInjection.Wrappers
{
	internal class EffectWrapper<TAction> : IEffect
	{
		private delegate Task HandleWithActionParameterAsyncHandler(TAction action, IDispatcher dispatcher);
		private delegate Task HandleWithActonParameterNoDispatcherAsyncHandler(TAction action);
		private delegate Task HandleWithoutActionParameterAsyncHandler(IDispatcher dispatcher);
		private readonly HandleWithActionParameterAsyncHandler HandleAsync;

		Task IEffect.HandleAsync(object action, IDispatcher dispatcher) => HandleAsync((TAction)action, dispatcher);
		bool IEffect.ShouldReactToAction(object action) => action is TAction;

		public EffectWrapper(object effectHostInstance, EffectMethodInfo effectMethodInfo)
		{
			HandleAsync =
				effectMethodInfo.RequiresActionParameterInMethod
				? CreateHandlerWithActionParameter(effectHostInstance, effectMethodInfo)
				: WrapEffectWithoutActionParameter(effectHostInstance, effectMethodInfo);
		}

		private static HandleWithActionParameterAsyncHandler WrapEffectWithoutActionParameter(
			object effectHostInstance,
			EffectMethodInfo effectMethodInfos)
		{
			HandleWithoutActionParameterAsyncHandler handler = CreateHandlerWithoutActionParameter(
				effectHostInstance,
				effectMethodInfos);

			return new HandleWithActionParameterAsyncHandler((action, dispatcher) => handler.Invoke(dispatcher));
		}

		private static HandleWithActionParameterAsyncHandler WrapInstanceEffectWithoutDispatcherParameter(
			object effectHostInstance,
			EffectMethodInfo effectMethodInfo)
		{
			HandleWithActonParameterNoDispatcherAsyncHandler handler = CreateInstanceHandlerWithoutDispatcherParameter(
				effectHostInstance,
				effectMethodInfo);

			return new HandleWithActionParameterAsyncHandler((action, dispatcher) => handler.Invoke(action));
		}

		private static HandleWithActionParameterAsyncHandler CreateHandlerWithActionParameter(
			object effectHostInstance,
			EffectMethodInfo effectMethodInfo)
		{
			if (effectHostInstance is null)
				return CreateStaticHandlerWithActionParameter(effectMethodInfo);
			else if (effectMethodInfo.RequiresDispatcherParameterInMethod)
				return CreateInstanceHandlerWithActionParameter(effectHostInstance, effectMethodInfo);
			else
				return WrapInstanceEffectWithoutDispatcherParameter(effectHostInstance, effectMethodInfo);
		}


		private static HandleWithActionParameterAsyncHandler CreateStaticHandlerWithActionParameter(
			EffectMethodInfo effectMethodInfo)
			=>
				(HandleWithActionParameterAsyncHandler)
					Delegate.CreateDelegate(
						type: typeof(HandleWithActionParameterAsyncHandler),
						method: effectMethodInfo.MethodInfo);

		private static HandleWithActionParameterAsyncHandler CreateInstanceHandlerWithActionParameter(
			object effectHostInstance,
			EffectMethodInfo effectMethodInfo)
			=>
				(HandleWithActionParameterAsyncHandler)
					Delegate.CreateDelegate(
						type: typeof(HandleWithActionParameterAsyncHandler),
						firstArgument: effectHostInstance,
						method: effectMethodInfo.MethodInfo);

		private static HandleWithoutActionParameterAsyncHandler CreateHandlerWithoutActionParameter(
			object effectHostInstance,
			EffectMethodInfo effectMethodInfo)
			=>
				effectHostInstance is null
				? CreateStaticHandlerWithoutActionParameter(effectMethodInfo)
				: CreateInstanceHandlerWithoutActionParameter(effectHostInstance, effectMethodInfo);

		private static HandleWithoutActionParameterAsyncHandler CreateStaticHandlerWithoutActionParameter(
			EffectMethodInfo effectMethodInfo)
			=>
				(HandleWithoutActionParameterAsyncHandler)
					Delegate.CreateDelegate(
						type: typeof(HandleWithoutActionParameterAsyncHandler),
						method: effectMethodInfo.MethodInfo);

		private static HandleWithoutActionParameterAsyncHandler CreateInstanceHandlerWithoutActionParameter(
			object effectHostInstance,
			EffectMethodInfo effectMethodInfo)
			=>
				(HandleWithoutActionParameterAsyncHandler)
					Delegate.CreateDelegate(
						type: typeof(HandleWithoutActionParameterAsyncHandler),
						firstArgument: effectHostInstance,
						method: effectMethodInfo.MethodInfo);

		private static HandleWithActonParameterNoDispatcherAsyncHandler CreateInstanceHandlerWithoutDispatcherParameter(
			object effectHostInstance,
			EffectMethodInfo effectMethodInfo)
			=>
				(HandleWithActonParameterNoDispatcherAsyncHandler)
					Delegate.CreateDelegate(
						type: typeof(HandleWithActonParameterNoDispatcherAsyncHandler),
						firstArgument: effectHostInstance,
						method: effectMethodInfo.MethodInfo);
	}
}
