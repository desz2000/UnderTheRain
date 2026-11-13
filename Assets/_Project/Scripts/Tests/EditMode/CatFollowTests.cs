using NUnit.Framework;
using UnityEngine;
using UnderTheRain.Companion;

namespace UnderTheRain.Tests.EditMode
{
    public class CatFollowTests
    {
        private CatConfigSO _config;
        private CatFollowModel _model;

        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<CatConfigSO>();
            _config.followOffset = new Vector2(-0.68f, 0f);
            _config.shoulderOffset = new Vector2(-0.20f, 0.52f);
            _config.followSpeed = 10f;
            _config.acceleration = 56f;
            _config.deceleration = 64f;
            _config.stopDistance = 0.08f;
            _config.gravity = 32f;
            _config.maxFallSpeed = 16f;
            _config.teleportDistance = 11f;
            _config.perchTransitionDuration = 0.18f;

            _model = new CatFollowModel(_config, Vector2.zero);
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_config);

        [Test]
        public void Step_FollowsCloselyBehindDante_WhenGroundedAndFar()
        {
            var input = new CatFollowInput
            {
                dantePosition = new Vector2(4f, 0f),
                facingDirection = 1,
                shouldPerch = false,
                grounded = true
            };

            _model.Step(0.05f, input);

            Assert.AreEqual(CatCompanionState.Follow, _model.State);
            Assert.Greater(_model.Velocity.x, 0f);
            Assert.AreEqual(1, _model.FacingDirection);
            Assert.AreEqual(-0.68f, _model.GetFollowTarget(Vector2.zero, 1).x, 0.001f);
        }

        [Test]
        public void Step_StaysCalmWithoutJumping_WhenGroundedEvenIfDanteIsAbove()
        {
            var input = new CatFollowInput
            {
                dantePosition = new Vector2(0.8f, 1.5f),
                facingDirection = 1,
                shouldPerch = false,
                grounded = true
            };

            _model.Step(0.02f, input);

            Assert.AreEqual(0f, _model.Velocity.y, 0.001f);
        }

        [Test]
        public void Step_TeleportsToFollowTarget_WhenTooFarFromDante()
        {
            var input = new CatFollowInput
            {
                dantePosition = new Vector2(20f, 3f),
                facingDirection = 1,
                shouldPerch = false,
                grounded = true
            };

            _model.Step(0.02f, input);

            Vector2 expected = _model.GetFollowTarget(input.dantePosition, 1);
            Assert.AreEqual(expected.x, _model.Position.x, 0.01f);
            Assert.AreEqual(expected.y, _model.Position.y, 0.01f);
            Assert.AreEqual(Vector2.zero, _model.Velocity);
        }

        [Test]
        public void Step_SmoothlyPerchesOnShoulder_AndDismountsCalmlyWhenCleared()
        {
            var perchInput = new CatFollowInput
            {
                dantePosition = new Vector2(2f, 1f),
                facingDirection = -1,
                shouldPerch = true,
                grounded = false
            };

            _model.Step(0.1f, perchInput);
            Assert.AreEqual(CatCompanionState.Perching, _model.State);

            _model.Step(0.15f, perchInput);
            Assert.AreEqual(CatCompanionState.Perched, _model.State);

            Vector2 expectedShoulder = _model.GetShoulderTarget(perchInput.dantePosition, -1);
            Assert.AreEqual(expectedShoulder.x, _model.Position.x, 0.01f);
            Assert.AreEqual(expectedShoulder.y, _model.Position.y, 0.01f);
            Assert.AreEqual(-1, _model.FacingDirection);

            // Dismount calmly back to Follow without hopping.
            perchInput.shouldPerch = false;
            perchInput.grounded = true;
            _model.Step(0.02f, perchInput);
            Assert.AreEqual(CatCompanionState.Follow, _model.State);
            Assert.AreEqual(0f, _model.Velocity.y, 0.001f);
        }
    }
}
