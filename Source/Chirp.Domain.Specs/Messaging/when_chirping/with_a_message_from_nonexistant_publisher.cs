using System;
using Bifrost.Commands;
using Bifrost.MSpec.Extensions;
using Chirp.Domain.Messaging.Commands;
using Machine.Specifications;

namespace Chirp.Domain.Specs.Messaging.when_chirping
{
    [Subject(Scenarios.WhenChirping)]
    public class with_a_message_from_nonexistant_publisher : given.a_scenario_with_a_chirp_message
    {
        static ChirpMessage chirp_message;
        static DateTime current_time;
        static CommandResult result;

        Establish context = () =>
                                {
                                    current_time = DateTime.UtcNow;

                                    chirp_message = new ChirpMessage
                                                        {
                                                            Publisher = publishers.valid_id_that_does_not_exist,
                                                            Message = messages.empty
                                                        };
                                };

        Because of = () =>
                         {
                             using (Bifrost.Time.SystemClock.SetNowTo(current_time))
                             {
                                 result = command_scenario.IsHandled(chirp_message);
                             }
                         };

        It should_be_an_unsuccessful_scenario = () => command_scenario.ShouldBeAnUnsuccessfulScenario();
        It should_not_chirp_the_message = () => command_scenario.GeneratedEvents.ShouldBeEmpty();
        It should_fail_validation = () => result.Invalid.ShouldBeTrue();
    }
}