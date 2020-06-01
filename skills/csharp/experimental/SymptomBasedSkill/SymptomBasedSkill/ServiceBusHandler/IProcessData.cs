using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static SymptomBasedSkill.ServiceBusHandler.ProcessData;

namespace SymptomBasedSkill.ServiceBusHandler
{
    public interface IProcessData
    {
        public Task<ServiceResponse> Process(string myPayload);
    }
}
