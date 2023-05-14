using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
 using SharpInterop.SDL2;
 using IntPtr = System.IntPtr;

 namespace Veldrid.StartupUtilities
{
    public static class VeldridStartup
    {
        public static void CreateWindowAndGraphicsDevice(
            WindowCreateInfo windowCI,
            out IntPtr window,
            out GraphicsDevice gd)
            => CreateWindowAndGraphicsDevice(
                windowCI,
                new GraphicsDeviceOptions(),
                GetPlatformDefaultBackend(),
                out window,
                out gd);

        public static void CreateWindowAndGraphicsDevice(
            WindowCreateInfo windowCI,
            GraphicsDeviceOptions deviceOptions,
            out IntPtr window,
            out GraphicsDevice gd)
            => CreateWindowAndGraphicsDevice(windowCI, deviceOptions, GetPlatformDefaultBackend(), out window, out gd);

        public static void CreateWindowAndGraphicsDevice(
            WindowCreateInfo windowCI,
            GraphicsDeviceOptions deviceOptions,
            GraphicsBackend preferredBackend,
            out IntPtr window,
            out GraphicsDevice gd)
        {
            SDL.SDL_Init(SDL.SDL_INIT_VIDEO);
            if (preferredBackend == GraphicsBackend.OpenGL || preferredBackend == GraphicsBackend.OpenGLES)
            {
                SetSDLGLContextAttributes(deviceOptions, preferredBackend);
            }

            window = CreateWindow(ref windowCI);
            gd = CreateGraphicsDevice(window, deviceOptions, preferredBackend);
        }


        public static IntPtr  CreateWindow(WindowCreateInfo windowCI) => CreateWindow(ref windowCI);

        public static IntPtr  CreateWindow(ref WindowCreateInfo windowCI)
        {
            SDL.SDL_WindowFlags flags = SDL.SDL_WindowFlags.SDL_WINDOW_OPENGL | SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE | GetWindowFlags(windowCI.WindowInitialState);
            if (windowCI.WindowInitialState != WindowState.Hidden)
            {
                flags |= SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN;
            }
            IntPtr window = SDL.SDL_CreateWindow(
                windowCI.WindowTitle,
                windowCI.X,
                windowCI.Y,
                windowCI.WindowWidth,
                windowCI.WindowHeight,
                flags
                );

            return window;
        }

        private static SDL.SDL_WindowFlags GetWindowFlags(WindowState state)
        {
            switch (state)
            {
                case WindowState.Normal:
                    return 0;
                case WindowState.FullScreen:
                    return SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN;
                case WindowState.Maximized:
                    return SDL.SDL_WindowFlags.SDL_WINDOW_MAXIMIZED;
                case WindowState.Minimized:
                    return SDL.SDL_WindowFlags.SDL_WINDOW_MINIMIZED;
                case WindowState.BorderlessFullScreen:
                    return SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN_DESKTOP;
                case WindowState.Hidden:
                    return SDL.SDL_WindowFlags.SDL_WINDOW_HIDDEN;
                default:
                    throw new VeldridException("Invalid WindowState: " + state);
            }
        }

        public static GraphicsDevice CreateGraphicsDevice(IntPtr window)
            => CreateGraphicsDevice(window, new GraphicsDeviceOptions(), GetPlatformDefaultBackend());
        public static GraphicsDevice CreateGraphicsDevice(IntPtr window, GraphicsDeviceOptions options)
            => CreateGraphicsDevice(window, options, GetPlatformDefaultBackend());
        public static GraphicsDevice CreateGraphicsDevice(IntPtr window, GraphicsBackend preferredBackend)
            => CreateGraphicsDevice(window, new GraphicsDeviceOptions(), preferredBackend);
        public static GraphicsDevice CreateGraphicsDevice(
            IntPtr window,
            GraphicsDeviceOptions options,
            GraphicsBackend preferredBackend)
        {
            switch (preferredBackend)
            {
                case GraphicsBackend.Direct3D11:
#if !EXCLUDE_D3D11_BACKEND
                    return CreateDefaultD3D11GraphicsDevice(options, window);
#else
                    throw new VeldridException("D3D11 support has not been included in this configuration of Veldrid");
#endif
                case GraphicsBackend.Vulkan:
#if !EXCLUDE_VULKAN_BACKEND
                    return CreateVulkanGraphicsDevice(options, window);
#else
                    throw new VeldridException("Vulkan support has not been included in this configuration of Veldrid");
#endif
                case GraphicsBackend.OpenGL:
#if !EXCLUDE_OPENGL_BACKEND
                    return CreateDefaultOpenGLGraphicsDevice(options, window, preferredBackend);
#else
                    throw new VeldridException("OpenGL support has not been included in this configuration of Veldrid");
#endif
                case GraphicsBackend.Metal:
#if !EXCLUDE_METAL_BACKEND
                    return CreateMetalGraphicsDevice(options, window);
#else
                    throw new VeldridException("Metal support has not been included in this configuration of Veldrid");
#endif
                case GraphicsBackend.OpenGLES:
#if !EXCLUDE_OPENGL_BACKEND
                    return CreateDefaultOpenGLGraphicsDevice(options, window, preferredBackend);
#else
                    throw new VeldridException("OpenGL support has not been included in this configuration of Veldrid");
#endif
                default:
                    throw new VeldridException("Invalid GraphicsBackend: " + preferredBackend);
            }
        }

        public static unsafe SwapchainSource GetSwapchainSource(IntPtr window)
        {
            IntPtr sdlHandle = window;
            SDL.SDL_SysWMinfo sysWmInfo = new SDL.SDL_SysWMinfo();
            SDL.SDL_GetVersion(out sysWmInfo.version);
            SDL.SDL_GetWindowWMInfo(sdlHandle, ref sysWmInfo);
            switch (sysWmInfo.subsystem)
            {
                case SDL.SDL_SYSWM_TYPE.SDL_SYSWM_WINDOWS:
                    return SwapchainSource.CreateWin32(sysWmInfo.info.win.window, sysWmInfo.info.win.hinstance);
                case SDL.SDL_SYSWM_TYPE.SDL_SYSWM_X11:
                    return SwapchainSource.CreateXlib(
                        sysWmInfo.info.x11.display,
                        sysWmInfo.info.x11.window);
                case SDL.SDL_SYSWM_TYPE.SDL_SYSWM_WAYLAND:
                    return SwapchainSource.CreateWayland(sysWmInfo.info.wl.display, sysWmInfo.info.wl.surface);
                case SDL.SDL_SYSWM_TYPE.SDL_SYSWM_COCOA:
                    IntPtr nsWindow = sysWmInfo.info.cocoa.window;
                    return SwapchainSource.CreateNSWindow(nsWindow);
                default:
                    throw new PlatformNotSupportedException("Cannot create a SwapchainSource for " + sysWmInfo.subsystem + ".");
            }
        }

#if !EXCLUDE_METAL_BACKEND
        private static unsafe GraphicsDevice CreateMetalGraphicsDevice(GraphicsDeviceOptions options, IntPtr window)
            => CreateMetalGraphicsDevice(options, window, options.SwapchainSrgbFormat);
        private static unsafe GraphicsDevice CreateMetalGraphicsDevice(
            GraphicsDeviceOptions options,
            IntPtr window,
            bool colorSrgb)
        {
            SwapchainSource source = GetSwapchainSource(window);
            
            SDL.SDL_GetWindowSize(window, out int Width, out int Height);
            
            
            SwapchainDescription swapchainDesc = new SwapchainDescription(
                source,
                (uint)Width, (uint)Height,
                options.SwapchainDepthFormat,
                options.SyncToVerticalBlank,
                colorSrgb);

            return GraphicsDevice.CreateMetal(options, swapchainDesc);
        }
#endif

        public static GraphicsBackend GetPlatformDefaultBackend()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return GraphicsBackend.Direct3D11;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return GraphicsDevice.IsBackendSupported(GraphicsBackend.Metal)
                    ? GraphicsBackend.Metal
                    : GraphicsBackend.OpenGL;
            }
            else
            {
                return GraphicsDevice.IsBackendSupported(GraphicsBackend.Vulkan)
                    ? GraphicsBackend.Vulkan
                    : GraphicsBackend.OpenGL;
            }
        }

#if !EXCLUDE_VULKAN_BACKEND
        public static unsafe GraphicsDevice CreateVulkanGraphicsDevice(GraphicsDeviceOptions options, IntPtr window)
            => CreateVulkanGraphicsDevice(options, window, false);
        public static unsafe GraphicsDevice CreateVulkanGraphicsDevice(
            GraphicsDeviceOptions options,
            IntPtr window,
            bool colorSrgb)
        {
            SDL.SDL_GetWindowSize(window, out int Width, out int Height);
            SwapchainDescription scDesc = new SwapchainDescription(
                GetSwapchainSource(window),
                (uint)Width,
                (uint)Height,
                options.SwapchainDepthFormat,
                options.SyncToVerticalBlank,
                colorSrgb);
            GraphicsDevice gd = GraphicsDevice.CreateVulkan(options, scDesc);

            return gd;
        }

        private static unsafe Veldrid.Vk.VkSurfaceSource GetSurfaceSource(SDL.SDL_SysWMinfo sysWmInfo)
        {
            switch (sysWmInfo.subsystem)
            {
                
                
                case SDL.SDL_SYSWM_TYPE.SDL_SYSWM_WINDOWS:
                    return Vk.VkSurfaceSource.CreateWin32(sysWmInfo.info.win.hinstance, sysWmInfo.info.win.window);
                case SDL.SDL_SYSWM_TYPE.SDL_SYSWM_X11:
                    return Vk.VkSurfaceSource.CreateXlib(
                        (Vulkan.Xlib.Display*)sysWmInfo.info.x11.display,
                        new Vulkan.Xlib.Window() { Value = sysWmInfo.info.x11.window});
                default:
                    throw new PlatformNotSupportedException("Cannot create a Vulkan surface for " + sysWmInfo.subsystem + ".");
            }
        }
#endif

#if !EXCLUDE_OPENGL_BACKEND
        public static unsafe GraphicsDevice CreateDefaultOpenGLGraphicsDevice(
            GraphicsDeviceOptions options,
            IntPtr window,
            GraphicsBackend backend)
        {
            SDL.SDL_ClearError();
            IntPtr sdlHandle = window;

            SDL.SDL_SysWMinfo sysWmInfo = new SDL.SDL_SysWMinfo();
            SDL.SDL_GetVersion(out sysWmInfo.version);
            SDL.SDL_GetWindowWMInfo(sdlHandle, ref sysWmInfo);

            SetSDLGLContextAttributes(options, backend);

            IntPtr contextHandle = SDL.SDL_GL_CreateContext(sdlHandle);
            string errorString = SDL.SDL_GetError();
            if (!string.IsNullOrEmpty(errorString))
            {
                throw new VeldridException(
                    $"Unable to create OpenGL Context: \"{errorString}\". This may indicate that the system does not support the requested OpenGL profile, version, or Swapchain format.");
            }

            int actualDepthSize;
            int result = SDL.SDL_GL_GetAttribute(SDL.SDL_GLattr.SDL_GL_DEPTH_SIZE, out actualDepthSize);
            int actualStencilSize;
            result = SDL.SDL_GL_GetAttribute(SDL.SDL_GLattr.SDL_GL_STENCIL_SIZE, out actualStencilSize);

            result = SDL.SDL_GL_SetSwapInterval(options.SyncToVerticalBlank ? 1 : 0);

            OpenGL.OpenGLPlatformInfo platformInfo = new OpenGL.OpenGLPlatformInfo(
                contextHandle,
                GL_GetProcAddress,
                context => SDL.SDL_GL_MakeCurrent(sdlHandle, context),
                () => SDL.SDL_GL_GetCurrentContext(),
                () => SDL.SDL_GL_MakeCurrent(IntPtr.Zero, IntPtr.Zero),
                SDL.SDL_GL_DeleteContext,
                () => SDL.SDL_GL_SwapWindow(sdlHandle),
                sync => SDL.SDL_GL_SetSwapInterval(sync ? 1 : 0));
            
            SDL.SDL_GetWindowSize(window, out int Width, out int Height);

            return GraphicsDevice.CreateOpenGL(
                options,
                platformInfo,
                (uint)Width,
                (uint)Height);
        }

        static IntPtr GL_GetProcAddress(string str)
        {
            return SDL.SDL_GL_GetProcAddress(str);
        }

        public static unsafe void SetSDLGLContextAttributes(GraphicsDeviceOptions options, GraphicsBackend backend)
        {
            if (backend != GraphicsBackend.OpenGL && backend != GraphicsBackend.OpenGLES)
            {
                throw new VeldridException(
                    $"{nameof(backend)} must be {nameof(GraphicsBackend.OpenGL)} or {nameof(GraphicsBackend.OpenGLES)}.");
            }

            SDL.SDL_GLcontext contextFlags = options.Debug
                ? SDL.SDL_GLcontext.SDL_GL_CONTEXT_DEBUG_FLAG | SDL.SDL_GLcontext.SDL_GL_CONTEXT_FORWARD_COMPATIBLE_FLAG
                : SDL.SDL_GLcontext.SDL_GL_CONTEXT_FORWARD_COMPATIBLE_FLAG;

            SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_CONTEXT_FLAGS, (int)contextFlags);

            (int major, int minor) = GetMaxGLVersion(backend == GraphicsBackend.OpenGLES);

            if (backend == GraphicsBackend.OpenGL)
            {
                SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_CONTEXT_PROFILE_MASK, (int)SDL.SDL_GLprofile.SDL_GL_CONTEXT_PROFILE_CORE);
                SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_CONTEXT_MAJOR_VERSION, major);
                SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_CONTEXT_MINOR_VERSION, minor);
            }
            else
            {
                SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_CONTEXT_PROFILE_MASK, (int)SDL.SDL_GLprofile.SDL_GL_CONTEXT_PROFILE_ES);
                SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_CONTEXT_MAJOR_VERSION, major);
                SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_CONTEXT_MINOR_VERSION, minor);
            }

            int depthBits = 0;
            int stencilBits = 0;
            if (options.SwapchainDepthFormat.HasValue)
            {
                switch (options.SwapchainDepthFormat)
                {
                    case PixelFormat.R16_UNorm:
                        depthBits = 16;
                        break;
                    case PixelFormat.D24_UNorm_S8_UInt:
                        depthBits = 24;
                        stencilBits = 8;
                        break;
                    case PixelFormat.R32_Float:
                        depthBits = 32;
                        break;
                    case PixelFormat.D32_Float_S8_UInt:
                        depthBits = 32;
                        stencilBits = 8;
                        break;
                    default:
                        throw new VeldridException("Invalid depth format: " + options.SwapchainDepthFormat.Value);
                }
            }

            int result = SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_DEPTH_SIZE, depthBits);
            result = SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_STENCIL_SIZE, stencilBits);

            SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_FRAMEBUFFER_SRGB_CAPABLE, options.SwapchainSrgbFormat ? 1 : 0);
        }
#endif

#if !EXCLUDE_D3D11_BACKEND
        public static GraphicsDevice CreateDefaultD3D11GraphicsDevice(
            GraphicsDeviceOptions options,
            IntPtr window)
        {
            SwapchainSource source = GetSwapchainSource(window);
            SDL.SDL_GetWindowSize(window, out int Width, out int Height);
            SwapchainDescription swapchainDesc = new SwapchainDescription(
                source,
                (uint)Width, (uint)Height,
                options.SwapchainDepthFormat,
                options.SyncToVerticalBlank,
                options.SwapchainSrgbFormat);

            return GraphicsDevice.CreateD3D11(options, swapchainDesc);
        }
#endif

        private static unsafe string GetString(byte* stringStart)
        {
            int characters = 0;
            while (stringStart[characters] != 0)
            {
                characters++;
            }

            return Encoding.UTF8.GetString(stringStart, characters);
        }

#if !EXCLUDE_OPENGL_BACKEND
        private static readonly object s_glVersionLock = new object();
        private static (int Major, int Minor)? s_maxSupportedGLVersion;
        private static (int Major, int Minor)? s_maxSupportedGLESVersion;

        private static (int Major, int Minor) GetMaxGLVersion(bool gles)
        {
            lock (s_glVersionLock)
            {
                (int Major, int Minor)? maxVer = gles ? s_maxSupportedGLESVersion : s_maxSupportedGLVersion;
                if (maxVer == null)
                {
                    maxVer = TestMaxVersion(gles);
                    if (gles) { s_maxSupportedGLESVersion = maxVer; }
                    else { s_maxSupportedGLVersion = maxVer; }
                }

                return maxVer.Value;
            }
        }

        private static (int Major, int Minor) TestMaxVersion(bool gles)
        {
            (int, int)[] testVersions = gles
                ? new[] { (3, 2), (3, 0) }
                : new[] { (4, 6), (4, 3), (4, 0), (3, 3), (3, 0) };

            foreach ((int major, int minor) in testVersions)
            {
                if (TestIndividualGLVersion(gles, major, minor)) { return (major, minor); }
            }

            return (0, 0);
        }

        private static unsafe bool TestIndividualGLVersion(bool gles, int major, int minor)
        {
            SDL.SDL_GLprofile profileMask = gles ? SDL.SDL_GLprofile.SDL_GL_CONTEXT_PROFILE_ES : SDL.SDL_GLprofile.SDL_GL_CONTEXT_PROFILE_CORE;

            SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_CONTEXT_PROFILE_MASK, (int)profileMask);
            SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_CONTEXT_MAJOR_VERSION, major);
            SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_CONTEXT_MINOR_VERSION, minor);

            IntPtr window = SDL.SDL_CreateWindow(
                string.Empty,
                0, 0,
                1, 1,
                SDL.SDL_WindowFlags.SDL_WINDOW_HIDDEN| SDL.SDL_WindowFlags.SDL_WINDOW_OPENGL);
            string errorString = SDL.SDL_GetError();

            if (window == IntPtr.Zero || !string.IsNullOrEmpty(errorString))
            {
                SDL.SDL_ClearError();
                Debug.WriteLine($"Unable to create version {major}.{minor} {profileMask} context.");
                return false;
            }

            IntPtr context = SDL.SDL_GL_CreateContext(window);

            if (!string.IsNullOrEmpty(errorString))
            {
                SDL.SDL_ClearError();
                Debug.WriteLine($"Unable to create version {major}.{minor} {profileMask} context.");
                SDL.SDL_DestroyWindow(window);
                return false;
            }

            SDL.SDL_GL_DeleteContext(context);
            SDL.SDL_DestroyWindow(window);
            return true;
        }
#endif
    }
}