using Bifrost.Commands;
using Bifrost.MSpec.Extensions;
using Chirp.Domain.Messaging.Commands;
using Machine.Specifications;

namespace Chirp.Domain.Specs.Messaging.when_deleting
{
    [Subject(Scenarios.WhenDeleting)]
    public class with_a_non_existant_publisher : given.a_scenario_with_a_delete_chirp_command
    {
        static DeleteChirp delete_chirp;
        static CommandResult result;

        Establish context = () =>
                                {
                                    delete_chirp = new DeleteChirp
                                                       {
                                                           PublishedBy = publishers.valid_id_that_does_not_exist,
                                                           ChirpToDelete = messages.valid_message_with_no_tags.Id
                                                       };
                                };

        Because of = () =>
                         {
                             result = command_scenario.IsHandled(delete_chirp);
                         };

        It should_be_an_unsuccessful_scenario = () => command_scenario.ShouldBeAnUnsuccessfulScenario();
        It should_not_delete_the_message = () => command_scenario.GeneratedEvents.ShouldBeEmpty();
        It should_fail_validation = () => result.Invalid.ShouldBeTrue();
    }
}