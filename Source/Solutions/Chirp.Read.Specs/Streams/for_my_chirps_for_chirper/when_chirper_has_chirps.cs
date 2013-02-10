﻿using System.Linq;
using Chirp.Read.Streams;
using Machine.Specifications;

namespace Chirp.Read.Specs.Streams.for_my_chirps_for_chirper
{
    [Subject(typeof (MyChirpsForChirper))]
    public class when_chirper_has_chirps : given.a_query
    {
        static OrderedStream chirp_stream;

        Establish context = () =>
                                {
                                    query.ChirperId = chirper_who_has_chirped;
                                };

        Because of = () => chirp_stream = query.Query.FirstOrDefault();

        It should_get_the_chirps_from_the_repository = () => chirp_repository.Verify(r => r.Query, Moq.Times.Once());
        It should_get_the_ordered_stream = () => chirp_stream.ShouldNotBeNull();
        It should_have_populated_the_stream_with_chirps = () => chirp_stream.Chirps().Any().ShouldBeTrue();
    }
}