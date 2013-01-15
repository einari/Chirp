﻿using Bifrost.Commands;
using Bifrost.MSpec.Extensions;
using Chirp.Domain.Messaging.Commands;
using Chirp.Events.Messaging;
using Machine.Specifications;

namespace Chirp.Domain.Specs.Messaging.when_deleting
{
    [Subject(Scenarios.WhenDeleting)]
    public class a_chirp : given.a_scenario_with_a_delete_chirp_command
    {
        static DeleteChirp delete_chirp;
        static CommandResult result;

        Establish context = () =>
        {
            delete_chirp = new DeleteChirp
            {
                PublishedBy = publishers.valid,
                ChirpToDelete = messages.valid_message_with_no_tags.Id
            };
        };

        Because of = () =>
        {
            result = command_scenario.IsHandled(delete_chirp);
        };

        It should_be_a_successful_scenario = () => command_scenario.ShouldBeASuccessfulScenario();
        It should_persist_the_events = () => command_scenario.HasGeneratedEvents.ShouldBeTrue();
        It should_delete_the_chirp = () => command_scenario.GeneratedEvents.ShouldHaveEvent<ChirpDeleted>().AtBeginning().Where(
             e =>
             {
                    e.DeletedChirp.ShouldEqual(messages.valid_message_with_no_tags.Id.Value);
                    e.PublishedBy.ShouldEqual(publishers.valid.Value);
             });
    }
}