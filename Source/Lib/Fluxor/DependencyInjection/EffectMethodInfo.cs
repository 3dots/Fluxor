﻿using Fluxor.Extensions;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Fluxor.DependencyInjection
{
	internal class EffectMethodInfo
	{
		public readonly Type HostClassType;
		public readonly MethodInfo MethodInfo;
		public readonly Type ActionType;
		public readonly bool RequiresActionParameterInMethod;
		public readonly bool RequiresDispatcherParameterInMethod;

		public EffectMethodInfo(
			Type hostClassType,
			EffectMethodAttribute attribute,
			MethodInfo methodInfo)
		{
			ParameterInfo[] methodParameters = methodInfo.GetParameters();

			HostClassType = hostClassType;
			MethodInfo = methodInfo;
			ActionType = attribute.ActionType ?? methodParameters[0].ParameterType;
			RequiresActionParameterInMethod = attribute.ActionType is null;
			RequiresDispatcherParameterInMethod = methodParameters.Length > 1;

			if (RequiresActionParameterInMethod && methodInfo.IsStatic && methodParameters.Length != 2)
				throw new ArgumentException(
					$"Static method must have 2 parameters (action, IDispatcher)"
						+ $" when [{nameof(EffectMethodAttribute)}] has no {nameof(EffectMethodAttribute.ActionType)} specified. "
						+ methodInfo.GetClassNameAndMethodName(),
					nameof(MethodInfo));

			if (!RequiresActionParameterInMethod && methodParameters.Length != 1)
				throw new ArgumentException(
					$"Method must have 1 parameter (IDispatcher)"
						+ $" when [{nameof(EffectMethodAttribute)}] has an {nameof(EffectMethodAttribute.ActionType)} specified. "
						+ methodInfo.GetClassNameAndMethodName(),
					nameof(methodInfo));

			Type lastParameterType = methodParameters[methodParameters.Length - 1].ParameterType;
			if (RequiresDispatcherParameterInMethod && lastParameterType != typeof(IDispatcher))
				throw new ArgumentException(
					$"The last parameter of a method should be an {nameof(IDispatcher)}"
						+ $" when decorated with an [{nameof(EffectMethodAttribute)}]. "
						+ methodInfo.GetClassNameAndMethodName(),
					nameof(methodInfo));

			if (methodInfo.ReturnType != typeof(Task))
				throw new ArgumentException(
					$"Effect methods must have a return type of {nameof(Task)}. " + methodInfo.GetClassNameAndMethodName(),
					nameof(methodInfo));
		}
	}
}
