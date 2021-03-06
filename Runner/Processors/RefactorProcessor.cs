// Copyright 2015 ThoughtWorks, Inc.
//
// This file is part of Gauge-CSharp.
//
// Gauge-CSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Gauge-CSharp is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with Gauge-CSharp.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Linq;
using Gauge.Messages;

namespace Gauge.CSharp.Runner.Processors
{
    internal class RefactorProcessor : IMessageProcessor
    {
        private readonly IStepRegistry _stepRegistry;

        public RefactorProcessor(IStepRegistry stepRegistry)
        {
            _stepRegistry = stepRegistry;
        }

        public Message Process(Message request)
        {
            var oldStepValue = request.RefactorRequest.OldStepValue.StepValue;
            var newStep = request.RefactorRequest.NewStepValue;

            var newStepValue = newStep.ParameterizedStepValue;
            var parameterPositions = request.RefactorRequest.ParamPositionsList;

            var methodInfo = _stepRegistry.MethodFor(oldStepValue);

            var refactorResponseBuilder = RefactorResponse.CreateBuilder();
            try
            {
                var filesChanged = RefactorHelper.Refactor(methodInfo, parameterPositions, newStep.ParametersList, newStepValue);
                refactorResponseBuilder.SetSuccess(true).AddFilesChanged(filesChanged.First());
            }
            catch (AggregateException ex)
            {
                refactorResponseBuilder.SetSuccess(false)
                    .SetError(ex.InnerExceptions.Select(exception => ex.Message).Aggregate((s, s1) => string.Concat(s, "; ", s1)));
            }
            catch (Exception ex)
            {
                refactorResponseBuilder.SetSuccess(false)
                    .SetError(ex.Message);
            }

            var refactorResponse =  refactorResponseBuilder.Build();

            return Message.CreateBuilder()
                .SetMessageId(request.MessageId)
                .SetMessageType(Message.Types.MessageType.RefactorResponse)
                .SetRefactorResponse(refactorResponse)
                .Build();
        }
    }
}