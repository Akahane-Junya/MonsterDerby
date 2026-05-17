namespace MonsterDerby.Domain.Course
{
    using System;
    using MonsterDerby.Domain.SharedKernel;

    public sealed class CourseDefinition
    {
        public CourseId CourseId { get; }
        public string Name { get; }
        public float LengthMeters { get; }
        public CourseSegment[] Segments { get; }

        public CourseDefinition(CourseId courseId, string name, float lengthMeters, CourseSegment[] segments)
        {
            CourseId = courseId;
            Name = Guard.NotNullOrEmpty(name, nameof(name));
            Guard.Positive(lengthMeters, nameof(lengthMeters));
            LengthMeters = lengthMeters;
            Segments = segments ?? throw new ArgumentNullException(nameof(segments));
        }
    }

    public sealed class CourseSegment
    {
        public float StartMeters { get; }
        public float EndMeters { get; }
        public string TerrainTag { get; }

        public CourseSegment(float startMeters, float endMeters, string terrainTag)
        {
            Guard.NonNegative(startMeters, nameof(startMeters));
            if (endMeters <= startMeters) throw new ArgumentOutOfRangeException(nameof(endMeters), endMeters, "End must be > Start.");
            StartMeters = startMeters;
            EndMeters = endMeters;
            TerrainTag = Guard.NotNullOrEmpty(terrainTag, nameof(terrainTag));
        }
    }
}