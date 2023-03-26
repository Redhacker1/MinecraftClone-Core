using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Numerics;
using System.Threading;
using Engine.Renderable;
using Engine.Rendering.Abstract;
using Engine.Rendering.Abstract.RenderStage;
using Engine.Rendering.Abstract.View;
using ImGuiNET;
using Veldrid;

namespace Engine.Objects.SceneSystem;

struct CullingMetrics
{
    public int AllowedMeshes = 0;
    public float TimeTaken = 0;

    public CullingMetrics()
    {
    }
}

class CullingPanel : ImGUIPanel
{

    CullingMetrics _metrics = new CullingMetrics();
    public override void CreateUI()
    {
        ImGui.Text($"Time to cull: {_metrics.TimeTaken}");
        ImGui.Text($"Entities rendered: {_metrics.AllowedMeshes}");
    }
    

    public void UpdateMetrics(CullingMetrics n)
    {
        _metrics = n;
    }
}

public class Scene
{

    ReaderWriterLockSlim ReadWriteLock = new ReaderWriterLockSlim();
    CullingPanel PanelCullingTime;

    List<RenderStage> stages = new List<RenderStage>();
    List<Instance> _instances = new List<Instance>();
    readonly CommandList list;

    public Scene()
    {
        PanelCullingTime = new CullingPanel();
        list = Engine.Renderer.Device.ResourceFactory.CreateCommandList();
    }


    /// <summary>
    /// This starts the process rolling of rendering the scene to the render target
    /// </summary>
    public void Render(Camera camera, RenderTarget renderTarget, float dt)
    {
        CameraInfo cameraInfo = new CameraInfo(camera);
        List<Instance> instance3Ds;
        Stopwatch stopwatch; 
        

        stopwatch = Stopwatch.StartNew();
        
        
        
        ReadWriteLock.EnterWriteLock();
        RemoveInstances(_instances);
        ReadWriteLock.ExitWriteLock();
        
        ReadWriteLock.EnterReadLock();
        instance3Ds = Cull(_instances, ref cameraInfo);
        ReadWriteLock.ExitReadLock();
        
        PanelCullingTime.UpdateMetrics(new CullingMetrics()
        {
            TimeTaken = (float)stopwatch.Elapsed.TotalMilliseconds,
            AllowedMeshes = instance3Ds.Count
        });
        Sort(instance3Ds, cameraInfo);
        DoRender(instance3Ds, ref cameraInfo, renderTarget, dt);
        
    }

    /// <summary>
    /// Used for actual Rendering code, happens after both the Culling, Sorting and Cleaning have been done
    /// </summary>
    /// <param name="instance3Ds">the Instances to render</param>
    /// <param name="camera">The camera to render from</param>
    /// <param name="renderTarget">The target to render to</param>
    protected virtual void DoRender( IReadOnlyList<Instance> instance3Ds, ref CameraInfo camera, RenderTarget renderTarget, float dt)
    {
        // Clear the screen, 
        list.Begin();
        list.PushDebugGroup("Clear Screen");
        renderTarget.Bind(list);
        list.ClearColorTarget(0, new RgbaFloat(.29804f,.29804f, .32157f, 1f));
        list.ClearDepthStencil(1, 0);
        list.PopDebugGroup();

        renderTarget.Bind(list);
        
        ReadWriteLock.EnterReadLock();
        foreach (RenderStage stage in stages)
        {
            stage.RunStage(new RenderState()
            {
                Target = Engine.MainFrameBuffer,
                Device = Engine.Renderer.Device, 
                GlobalCommandList = list
            }, renderTarget, 0, instance3Ds);
        }
        ReadWriteLock.ExitReadLock();

        Engine.Renderer.RenderImgGui(dt, list);
        list.End();
        Engine.Renderer.RunCommandList(list);
        Engine.Renderer.SwapBuffers();
        
        
    }

    public void AddInstance(Instance instance3D)
    {
        ReadWriteLock.EnterWriteLock();
        if (_instances.Contains(instance3D) == false)
        {
            _instances.Add(instance3D);
        }
        ReadWriteLock.ExitWriteLock();

    }

    public void RemoveInstance(Instance instance3D)
    {
        ReadWriteLock.EnterWriteLock();
        if (_instances.Contains(instance3D) == true)
        {
            _instances.Remove(instance3D);
        }
        ReadWriteLock.ExitWriteLock();
    }
    
    public void AddStage(RenderStage stage)
    {
        ReadWriteLock.EnterWriteLock();
        if (stages.Contains(stage) == false)
        {
            stages.Add(stage);
        }
        ReadWriteLock.ExitWriteLock();

    }
    
    public void RemoveStage(RenderStage stage)
    {
        ReadWriteLock.EnterWriteLock();
        if (stages.Contains(stage) == true)
        { 
            stages.Remove(stage);
        }
        ReadWriteLock.ExitWriteLock();
    }

    static void RemoveInstances(List<Instance> instance3Ds)
    {
        for (int index = 0; index < instance3Ds.Count; index++) 
        {
            Instance instance3D = instance3Ds[index];
            if (instance3D._baseRenderableElement?.Disposed == true)
            {
                instance3Ds[index] = instance3Ds[^1];
                instance3Ds.RemoveAt(instance3Ds.Count - 1);
            }
        }
    }

    protected virtual List<Instance> Cull(IEnumerable<Instance> instances, ref CameraInfo cameraInfo) 
    {
        List<Instance> instance3Ds = new List<Instance>(0);

        foreach (Instance instance in instances)
        {
            Instance3D currentInstance = (Instance3D) instance;
            // Check if the WeakReference is valid and if the mesh it points to has not been disposed. 
            if (currentInstance?._baseRenderableElement?.Disposed == false)
            {
                if (currentInstance.ShouldRender(ref cameraInfo) && currentInstance._baseRenderableElement.VertexElements > 0)
                {
                    instance3Ds.Add(currentInstance);
                }
            }
        }

        return instance3Ds;
    }
    
    /// <summary>
    ///  The sorting algorithm, by default sorts position in world space using <see cref="List{T}.Sort()"/>>.
    /// </summary>
    /// <param name="instances"></param>
    /// <param name="cameraInfo">Camera reference</param>
    protected virtual void Sort(List<Instance> instances, CameraInfo cameraInfo)
    {
        instances.Sort((item, comparator) => SortCriteria(item, comparator, ref cameraInfo));
    }

    protected virtual int SortCriteria(Instance first, Instance second, ref CameraInfo cameraInfo)
    {
        float distance1 = Vector3.DistanceSquared(cameraInfo.CameraTransform.Position, first.Position);
        float distance2 = Vector3.DistanceSquared(cameraInfo.CameraTransform.Position, second.Position);
            
        if (Math.Abs(distance1 - distance2) > float.Epsilon)
        {
            return distance1 > distance2 ? 1 : -1;
        }
        return 0;
    }
}