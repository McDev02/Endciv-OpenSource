using UnityEngine;
using UnityEngine.Rendering;
using System;

namespace McLOD
{
    public class McLODSkinnedMesh : McLODEntity<McLODSettingSkinnedMesh>
    {
        [SerializeField] SkinnedMeshRenderer SkinnedMesh;
        [SerializeField] Animator Animator;
        [SerializeField] MeshFilter MeshFilter;
        [SerializeField] MeshRenderer MeshRenderer;

        protected override void ApplyLODState(McLODSettingSkinnedMesh state)
        {
            if (state.cull)
            {
                Animator.enabled = false;
                SkinnedMesh.enabled = false;
                MeshRenderer.enabled = false;
                return;
            }

            if (state.showSkinnedMesh)
            {
                Animator.enabled = true;
                SkinnedMesh.enabled = true;
                SkinnedMesh.shadowCastingMode = state.shadowMode;
                MeshRenderer.enabled = false;
            }
            else
            {
                Animator.enabled = false;
                SkinnedMesh.enabled = false;
                MeshRenderer.enabled = true;
                MeshRenderer.shadowCastingMode = state.shadowMode;
            }
        }

        void Awake()
        {
            if (SkinnedMesh == null) SkinnedMesh = GetComponentInChildren<SkinnedMeshRenderer>();
        }

    }
    [Serializable]
    public class McLODSettingSkinnedMesh : McLODSetting
    {
        public bool showSkinnedMesh = true;
        public ShadowCastingMode shadowMode;
    }
}