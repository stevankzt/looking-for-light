using System.Collections.Generic;

namespace LookingForLight;

public class SceneManager
{
   private readonly Stack<Iscene> sceneStack;

   public SceneManager()
   {
      sceneStack = new();
   }

   public void AddScene(Iscene scene)
   {
      scene.Load();
      
      sceneStack.Push(scene);
   }

   public void RemoveScene(Iscene scene)
   {
      sceneStack.Pop();
   }

   public Iscene GetCurrentScene()
   {
      return sceneStack.Peek();
   }
}