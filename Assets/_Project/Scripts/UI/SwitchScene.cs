using Eflatun.SceneReference;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SwitchScene : MonoBehaviour
{
   [SerializeField] private SceneReference targetScene;  // 在 Inspector 中赋值目标场景

   // 调用此方法来加载场景
   public void LoadTargetScene()
   {
      if (targetScene.State == SceneReferenceState.Regular)
      {
         // 异步加载场景（非阻塞）
         SceneManager.LoadSceneAsync(targetScene.Path);
      }
      else
      {
         // 输出诊断信息（例如，场景未在构建中）
         Debug.LogError("无法加载场景：" + targetScene.Path + ", 状态：" + targetScene.State);
      }
   }
}