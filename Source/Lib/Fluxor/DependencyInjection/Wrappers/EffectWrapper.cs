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
			HandleAsync =
				effectMethodInfo.RequiresActionParameterInMethod
				? CreateHandlerWithActionParameter(effectHostInstance, effectMethodInfo)
				: WrapEffectWithoutActionParameter(effectHostInstance, effectMethodInfo);
		}

		private static HandleWithActionParameterAsyncHandler WrapEffectWithoutActionParameter(
			object effectHostInstance,
			EffectMethodInfo effectMethodInfo)
			=> effectMethodInfo.IsVoid ?
				WrapVoidEffectWithoutActionParameter(effectHostInstance, effectMethodInfo) :
				WrapAsyncEffectWithoutActionParameter(effectHostInstance, effectMethodInfo);

		private static HandleWithActionParameterAsyncHandler WrapVoidEffectWithoutActionParameter(
			object effectHostInstance,
			EffectMethodInfo effectMethodInfo)
		{
			HandleWithoutActionParameterVoidHandler handler = CreateVoidHandlerWithoutActionParameter(
				effectHostInstance,
				effectMethodInfo);

			return new HandleWithActionParameterAsyncHandler((action, dispatcher) =>
			{
				handler.Invoke(dispatcher);
				return Task.CompletedTask;
			});
		}

		private static HandleWithActionParameterAsyncHandler WrapAsyncEffectWithoutActionParameter(
			object effectHostInstance,
			EffectMethodInfo effectMethodInfo)
		{
			HandleWithoutActionParameterAsyncHandler handler = CreateHandlerWithoutActionParameter(
				effectHostInstance,
				effectMethodInfo);

			return new HandleWithActionParameterAsyncHandler((action, dispatcher) => handler.Invoke(dispatcher));
		}

		private static HandleWithActionParameterAsyncHandler WrapInstanceEffectWithoutDispatcherParameter(
			object effectHostInstance,
			EffectMethodInfo effectMethodInfo)
			=> effectMethodInfo.IsVoid ?
				WrapInstanceVoidEffectWithoutDispatcherParameter(effectHostInstance, effectMethodInfo) :
				WrapInstanceAsyncEffectWithoutDispatcherParameter(effectHostInstance, effectMethodInfo);

		private static HandleWithActionParameterAsyncHandler WrapInstanceAsyncEffectWithoutDispatcherParameter(
			object effectHostInstance,
			EffectMethodInfo effectMethodInfo)
		{
			HandleWithActonParameterNoDispatcherAsyncHandler handler = CreateInstanceHandlerWithoutDispatcherParameter(
				effectHostInstance,
				effectMethodInfo);

			return new HandleWithActionParameterAsyncHandler((action, dispatcher) => handler.Invoke(action));
		}

		private static HandleWithActionParameterAsyncHandler WrapInstanceVoidEffectWithoutDispatcherParameter(
			object effectHostInstance,
			EffectMethodInfo effectMethodInfo)
		{
			HandleWithActionParameterNoDispatcherVoidHandler handler = CreateInstanceVoidHandlerWithoutDispatcherParameter(
				effectHostInstance,
				effectMethodInfo);

			return new HandleWithActionParameterAsyncHandler((action, dispatcher) =>
			{
				handler.Invoke(action);
				return Task.CompletedTask;
			});
		}

		private static HandleWithActionParameterAsyncHandler WrapStaticVoidEffectWithActionParameter(
			EffectMethodInfo effectMethodInfo)
		{
			HandleWithActionParameterVoidHandler handler = CreateStaticVoidHandlerWithActionParameter(effectMethodInfo);

			return new HandleWithActionParameterAsyncHandler((action, dispatcher) =>
			{
				handler.Invoke(action, dispatcher);
				return Task.CompletedTask;
			});
		}

		private static HandleWithActionParameterAsyncHandler WrapInstanceVoidEffectWithActionParameter(
			object effectHostInstance,
			EffectMethodInfo effectMethodInfo)
		{
			HandleWithActionParameterVoidHandler handler = CreateInstanceVoidHandler(effectHostInstance, effectMethodInfo);

			return new HandleWithActionParameterAsyncHandler((action, dispatcher) =>
			{
				handler.Invoke(action, dispatcher);
				return Task.CompletedTask;
			});
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
			=> effectMethodInfo.IsVoid ?
				WrapStaticVoidEffectWithActionParameter(effectMethodInfo) :
				CreateStaticAsyncHandlerWithActionParameter(effectMethodInfo);

		private static HandleWithActionParameterAsyncHandler CreateStaticAsyncHandlerWithActionParameter(
			EffectMethodInfo effectMethodInfo)
			=>
				(HandleWithActionParameterAsyncHandler)
					Delegate.CreateDelegate(
						type: typeof(HandleWithActionParameterAsyncHandler),
						method: effectMethodInfo.MethodInfo);

		private static HandleWithActionParameterAsyncHandler CreateInstanceHandlerWithActionParameter(
			object effectHostInstance,
			EffectMethodInfo effectMethodInfo)
			=> effectMethodInfo.IsVoid ?
				WrapInstanceVoidEffectWithActionParameter(effectHostInstance, effectMethodInfo) :
				CreateInstanceAsyncHandlerWithActionParameter(effectHostInstance, effectMethodInfo);

		private static HandleWithActionParameterAsyncHandler CreateInstanceAsyncHandlerWithActionParameter(
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

		private static HandleWithoutActionParameterVoidHandler CreateVoidHandlerWithoutActionParameter(
			object effectHostInstance,
			EffectMethodInfo effectMethodInfo)
			=>
				effectHostInstance is null
				? CreateStaticVoidHandlerWithoutActionParameter(effectMethodInfo)
				: CreateInstanceVoidHandlerWithoutActionParameter(effectHostInstance, effectMethodInfo);

		private static HandleWithoutActionParameterAsyncHandler CreateStaticHandlerWithoutActionParameter(
			EffectMethodInfo effectMethodInfo)
			=>
				(HandleWithoutActionParameterAsyncHandler)
					Delegate.CreateDelegate(
						type: typeof(HandleWithoutActionParameterAsyncHandler),
						method: effectMethodInfo.MethodInfo);

		private static HandleWithoutActionParameterVoidHandler CreateStaticVoidHandlerWithoutActionParameter(
			EffectMethodInfo effectMethodInfo)
			=>
				(HandleWithoutActionParameterVoidHandler)
					Delegate.CreateDelegate(
						type: typeof(HandleWithoutActionParameterVoidHandler),
						method: effectMethodInfo.MethodInfo);

		private static HandleWithActionParameterVoidHandler CreateStaticVoidHandlerWithActionParameter(
			EffectMethodInfo effectMethodInfo)
			=>
				(HandleWithActionParameterVoidHandler)
					Delegate.CreateDelegate(
						type: typeof(HandleWithActionParameterVoidHandler),
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

		private static HandleWithoutActionParameterVoidHandler CreateInstanceVoidHandlerWithoutActionParameter(
			object effectHostInstance,
			EffectMethodInfo effectMethodInfo)
			=>
				(HandleWithoutActionParameterVoidHandler)
					Delegate.CreateDelegate(
						type: typeof(HandleWithoutActionParameterVoidHandler),
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

		private static HandleWithActionParameterVoidHandler CreateInstanceVoidHandler(
			object effectHostInstance,
			EffectMethodInfo effectMethodInfo)
			=>
				(HandleWithActionParameterVoidHandler)
					Delegate.CreateDelegate(
						type: typeof(HandleWithActionParameterVoidHandler),
						firstArgument: effectHostInstance,
						method: effectMethodInfo.MethodInfo);

		private static HandleWithActionParameterNoDispatcherVoidHandler CreateInstanceVoidHandlerWithoutDispatcherParameter(
			object effectHostInstance,
			EffectMethodInfo effectMethodInfo)
			=>
				(HandleWithActionParameterNoDispatcherVoidHandler)
					Delegate.CreateDelegate(
						type: typeof(HandleWithActionParameterNoDispatcherVoidHandler),
						firstArgument: effectHostInstance,
						method: effectMethodInfo.MethodInfo);
	}
}
