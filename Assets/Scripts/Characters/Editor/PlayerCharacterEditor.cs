using Characters.MonoBehaviours;
using UnityEditor;
using UnityEngine;

namespace Characters.Editor
{
    [CustomEditor(typeof(PlayerCharacter))]
    public class PlayerCharacterEditor : UnityEditor.Editor
    {
        private SerializedProperty _spriteRendererProp;
        private SerializedProperty _mDamageableProp;
        private SerializedProperty _mMeleeDamagerProp;
        private SerializedProperty _mCameraFollowTargetProp;

        private SerializedProperty _mMaxSpeedProp;
        private SerializedProperty _mGroundAccelerationProp;
        private SerializedProperty _mGroundDecelerationProp;
        private SerializedProperty _mPushingSpeedProportionProp;

        private SerializedProperty _mAirborneAccelProportionProp;
        private SerializedProperty _mAirborneDecelProportionProp;
        private SerializedProperty _mGravityProp;
        private SerializedProperty _mJumpSpeedProp;
        private SerializedProperty _mJumpAbortSpeedReductionProp;
        private SerializedProperty _bounceStrengthProp;

        private SerializedProperty _mHurtJumpAngleProp;
        private SerializedProperty _mHurtJumpSpeedProp;
        private SerializedProperty _mFlickeringDurationProp;

        private SerializedProperty _mFootstepGrassPlayerProp;
        private SerializedProperty _mFootstepMetalPlayerProp;
        private SerializedProperty _mLandingAudioPlayerProp;
        private SerializedProperty _mHurtAudioPlayerProp;
        private SerializedProperty _mMeleeAttackAudioPlayerProp;
        private SerializedProperty _mRangedAttackAudioPlayerProp;

        private SerializedProperty _mCameraHorizontalFacingOffsetProp;
        private SerializedProperty _mCameraHorizontalSpeedOffsetProp;
        private SerializedProperty _mCameraVerticalInputOffsetProp;
        private SerializedProperty _mMaxHorizontalDeltaDampTimeProp;
        private SerializedProperty _mMaxVerticalDeltaDampTimeProp;
        private SerializedProperty _mVerticalCameraOffsetDelayProp;

        private SerializedProperty _mSpriteOriginallyFacesLeftProp;
        
        private SerializedProperty _worldTypeProp;

        private bool _mReferencesFoldout;
        private bool _mMovementSettingsFoldout;
        private bool _mAirborneSettingsFoldout;
        private bool _mHurtSettingsFoldout;
        private bool _mMeleeSettingsFoldout;
        private bool _mAudioSettingsFoldout;
        private bool _mCameraFollowSettingsFoldout;
        private bool _mMiscSettingsFoldout;

        private readonly GUIContent _mSpriteRendererContent = new GUIContent("spriteRenderer");
        private readonly GUIContent _mDamageableContent = new GUIContent("damageable");
        private readonly GUIContent _mMeleeDamagerContent = new GUIContent("meleeDamager");
        private readonly GUIContent _mCameraFollowTargetContent = new GUIContent("cameraFollowTarget");

        private readonly GUIContent _mMaxSpeedContent = new GUIContent("maxSpeed");
        private readonly GUIContent _mGroundAccelerationContent = new GUIContent("groundAcceleration");
        private readonly GUIContent _mGroundDecelerationContent = new GUIContent("groundDeceleration");
        private readonly GUIContent _mPushingSpeedProportionContent = new GUIContent("Pushing Speed Proportion");

        private readonly GUIContent _mAirborneAccelProportionContent = new GUIContent("Airborne Accel Proportion");
        private readonly GUIContent _mAirborneDecelProportionContent = new GUIContent("Airborne Decel Proportion");
        private readonly GUIContent _mGravityContent = new GUIContent("Gravity");
        private readonly GUIContent _mJumpSpeedContent = new GUIContent("Jump Speed");
        private readonly GUIContent _mJumpAbortSpeedReductionContent = new GUIContent("Jump Abort Speed Reduction");
        private readonly GUIContent _bounceStrengthContent = new GUIContent("Bounce Strength");

        private readonly GUIContent _mHurtJumpAngleContent = new GUIContent("Hurt Jump Angle");
        private readonly GUIContent _mHurtJumpSpeedContent = new GUIContent("Hurt Jump Speed");

        private readonly GUIContent _mFlickeringDurationContent = new GUIContent("Flicking Duration",
            "When the player is hurt she becomes invulnerable for a short time and the SpriteRenderer flickers on and off to indicate this.  " +
            "This field is the duration in seconds the SpriteRenderer stays either on or off whilst flickering.  " +
            "To adjust the duration of invulnerability see the Damageable component.");


        private readonly GUIContent _mFootstepGrassPlayerContent = new GUIContent("Footstep Grass Audio Player");
        private readonly GUIContent _mFootstepMetalPlayerContent = new GUIContent("Footstep Metal Audio Player");
        private readonly GUIContent _mLandingAudioPlayerContent = new GUIContent("Landing Audio Player");
        private readonly GUIContent _mHurtAudioPlayerContent = new GUIContent("Hurt Audio Player");
        private readonly GUIContent _mMeleeAttackAudioPlayerContent = new GUIContent("Melee Attack Audio Player");
        private readonly GUIContent _mRangedAttackAudioPlayerContent = new GUIContent("Ranged Attack Audio Player");

        private readonly GUIContent _mCameraHorizontalFacingOffsetContent =
            new GUIContent("Camera Horizontal Facing Offset");

        private readonly GUIContent _mCameraHorizontalSpeedOffsetContent =
            new GUIContent("Camera Horizontal Speed Offset");

        private readonly GUIContent _mCameraVerticalInputOffsetContent = new GUIContent("Camera Vertical Input Offset");

        private readonly GUIContent _mMaxHorizontalDeltaDampTimeContent =
            new GUIContent("Max Horizontal Delta Damp Time");

        private readonly GUIContent _mMaxVerticalDeltaDampTimeContent = new GUIContent("Max Vertical Delta Damp Time");
        private readonly GUIContent _mVerticalCameraOffsetDelayContent = new GUIContent("Vertical Camera Offset Delay");

        private readonly GUIContent _mSpriteOriginallyFacesLeftContent = new GUIContent("Sprite Originally Faces Left");
        
        private readonly GUIContent _worldTypeContent = new GUIContent("World Type");

        private readonly GUIContent _mReferencesContent = new GUIContent("References");
        private readonly GUIContent _mMovementSettingsContent = new GUIContent("Movement Settings");
        private readonly GUIContent _mAirborneSettingsContent = new GUIContent("Airborne Settings");
        private readonly GUIContent _mHurtSettingsContent = new GUIContent("Hurt Settings");

        private readonly GUIContent _mAudioSettingsContent = new GUIContent("Audio Settings");
        private readonly GUIContent _mCameraFollowSettingsContent = new GUIContent("Camera Follow Settings");
        private readonly GUIContent _mMiscSettingsContent = new GUIContent("Misc Settings");

        protected void OnEnable()
        {
            _spriteRendererProp = serializedObject.FindProperty("spriteRenderer");
            _mDamageableProp = serializedObject.FindProperty("damageable");
            _mMeleeDamagerProp = serializedObject.FindProperty("meleeDamager");
            _mCameraFollowTargetProp = serializedObject.FindProperty("cameraFollowTarget");

            _mMaxSpeedProp = serializedObject.FindProperty("maxSpeed");
            _mGroundAccelerationProp = serializedObject.FindProperty("groundAcceleration");
            _mGroundDecelerationProp = serializedObject.FindProperty("groundDeceleration");
            _mPushingSpeedProportionProp = serializedObject.FindProperty("pushingSpeedProportion");

            _mAirborneAccelProportionProp = serializedObject.FindProperty("airborneAccelProportion");
            _mAirborneDecelProportionProp = serializedObject.FindProperty("airborneDecelProportion");
            _mGravityProp = serializedObject.FindProperty("gravity");
            _mJumpSpeedProp = serializedObject.FindProperty("jumpSpeed");
            _mJumpAbortSpeedReductionProp = serializedObject.FindProperty("jumpAbortSpeedReduction");
            _bounceStrengthProp = serializedObject.FindProperty("bounceStrength");

            _mHurtJumpAngleProp = serializedObject.FindProperty("hurtJumpAngle");
            _mHurtJumpSpeedProp = serializedObject.FindProperty("hurtJumpSpeed");
            _mFlickeringDurationProp = serializedObject.FindProperty("flickeringDuration");

            _mFootstepGrassPlayerProp = serializedObject.FindProperty("footstepGrassAudioPlayer");
            _mFootstepMetalPlayerProp = serializedObject.FindProperty("footstepMetalAudioPlayer");
            _mLandingAudioPlayerProp = serializedObject.FindProperty("landingAudioPlayer");
            _mHurtAudioPlayerProp = serializedObject.FindProperty("hurtAudioPlayer");
            _mMeleeAttackAudioPlayerProp = serializedObject.FindProperty("meleeAttackAudioPlayer");
            _mRangedAttackAudioPlayerProp = serializedObject.FindProperty("rangedAttackAudioPlayer");

            _mCameraHorizontalFacingOffsetProp = serializedObject.FindProperty("cameraHorizontalFacingOffset");
            _mCameraHorizontalSpeedOffsetProp = serializedObject.FindProperty("cameraHorizontalSpeedOffset");
            _mCameraVerticalInputOffsetProp = serializedObject.FindProperty("cameraVerticalInputOffset");
            _mMaxHorizontalDeltaDampTimeProp = serializedObject.FindProperty("maxHorizontalDeltaDampTime");
            _mMaxVerticalDeltaDampTimeProp = serializedObject.FindProperty("maxVerticalDeltaDampTime");
            _mVerticalCameraOffsetDelayProp = serializedObject.FindProperty("verticalCameraOffsetDelay");

            _mSpriteOriginallyFacesLeftProp = serializedObject.FindProperty("spriteOriginallyFacesLeft");
            _worldTypeProp = serializedObject.FindProperty("worldType");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUI.indentLevel++;

            _mReferencesFoldout = EditorGUILayout.Foldout(_mReferencesFoldout, _mReferencesContent);

            if (_mReferencesFoldout)
            {
                EditorGUILayout.PropertyField(_spriteRendererProp, _mSpriteRendererContent);
                EditorGUILayout.PropertyField(_mDamageableProp, _mDamageableContent);
                EditorGUILayout.PropertyField(_mMeleeDamagerProp, _mMeleeDamagerContent);
                EditorGUILayout.PropertyField(_mCameraFollowTargetProp, _mCameraFollowTargetContent);
            }

            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUI.indentLevel++;

            _mMovementSettingsFoldout = EditorGUILayout.Foldout(_mMovementSettingsFoldout, _mMovementSettingsContent);

            if (_mMovementSettingsFoldout)
            {
                EditorGUILayout.PropertyField(_mMaxSpeedProp, _mMaxSpeedContent);
                EditorGUILayout.PropertyField(_mGroundAccelerationProp, _mGroundAccelerationContent);
                EditorGUILayout.PropertyField(_mGroundDecelerationProp, _mGroundDecelerationContent);
                EditorGUILayout.PropertyField(_mPushingSpeedProportionProp, _mPushingSpeedProportionContent);
            }

            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUI.indentLevel++;

            _mAirborneSettingsFoldout = EditorGUILayout.Foldout(_mAirborneSettingsFoldout, _mAirborneSettingsContent);

            if (_mAirborneSettingsFoldout)
            {
                EditorGUILayout.PropertyField(_mAirborneAccelProportionProp, _mAirborneAccelProportionContent);
                EditorGUILayout.PropertyField(_mAirborneDecelProportionProp, _mAirborneDecelProportionContent);
                EditorGUILayout.PropertyField(_mGravityProp, _mGravityContent);
                EditorGUILayout.PropertyField(_mJumpSpeedProp, _mJumpSpeedContent);
                EditorGUILayout.PropertyField(_mJumpAbortSpeedReductionProp, _mJumpAbortSpeedReductionContent);
                EditorGUILayout.PropertyField(_bounceStrengthProp, _bounceStrengthContent);
            }

            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUI.indentLevel++;

            _mHurtSettingsFoldout = EditorGUILayout.Foldout(_mHurtSettingsFoldout, _mHurtSettingsContent);

            if (_mHurtSettingsFoldout)
            {
                EditorGUILayout.PropertyField(_mHurtJumpAngleProp, _mHurtJumpAngleContent);
                EditorGUILayout.PropertyField(_mHurtJumpSpeedProp, _mHurtJumpSpeedContent);
                EditorGUILayout.PropertyField(_mFlickeringDurationProp, _mFlickeringDurationContent);
            }

            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUI.indentLevel++;

            _mAudioSettingsFoldout = EditorGUILayout.Foldout(_mAudioSettingsFoldout, _mAudioSettingsContent);

            if (_mAudioSettingsFoldout)
            {
                EditorGUILayout.PropertyField(_mFootstepGrassPlayerProp, _mFootstepGrassPlayerContent);
                EditorGUILayout.PropertyField(_mFootstepMetalPlayerProp, _mFootstepMetalPlayerContent);
                EditorGUILayout.PropertyField(_mLandingAudioPlayerProp, _mLandingAudioPlayerContent);
                EditorGUILayout.PropertyField(_mHurtAudioPlayerProp, _mHurtAudioPlayerContent);
                EditorGUILayout.PropertyField(_mMeleeAttackAudioPlayerProp, _mMeleeAttackAudioPlayerContent);
                EditorGUILayout.PropertyField(_mRangedAttackAudioPlayerProp, _mRangedAttackAudioPlayerContent);
            }

            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUI.indentLevel++;

            _mCameraFollowSettingsFoldout =
                EditorGUILayout.Foldout(_mCameraFollowSettingsFoldout, _mCameraFollowSettingsContent);

            if (_mCameraFollowSettingsFoldout)
            {
                EditorGUILayout.PropertyField(_mCameraHorizontalFacingOffsetProp,
                    _mCameraHorizontalFacingOffsetContent);
                EditorGUILayout.PropertyField(_mCameraHorizontalSpeedOffsetProp, _mCameraHorizontalSpeedOffsetContent);
                EditorGUILayout.PropertyField(_mCameraVerticalInputOffsetProp, _mCameraVerticalInputOffsetContent);
                EditorGUILayout.PropertyField(_mMaxHorizontalDeltaDampTimeProp, _mMaxHorizontalDeltaDampTimeContent);
                EditorGUILayout.PropertyField(_mMaxVerticalDeltaDampTimeProp, _mMaxVerticalDeltaDampTimeContent);
                EditorGUILayout.PropertyField(_mVerticalCameraOffsetDelayProp, _mVerticalCameraOffsetDelayContent);
            }

            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUI.indentLevel++;

            _mMiscSettingsFoldout = EditorGUILayout.Foldout(_mMiscSettingsFoldout, _mMiscSettingsContent);

            if (_mMiscSettingsFoldout)
            {
                EditorGUILayout.PropertyField(_mSpriteOriginallyFacesLeftProp, _mSpriteOriginallyFacesLeftContent);
                EditorGUILayout.PropertyField(_worldTypeProp, _worldTypeContent);
            }

            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }
    }
}