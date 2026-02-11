import { createRouter, createWebHistory } from "vue-router";

// Overview pages
import CrowdOverview from "@/views/overview/CrowdOverview.vue";
import FenceOverview from "@/views/overview/FenceOverview.vue";
import ParkingOverview from "@/views/overview/ParkingOverview.vue";
import TrafficOverview from "@/views/overview/TrafficOverview.vue";

// Report pages
import AdmissionReport from "@/views/report/AdmissionReport.vue";
import CrowdReport from "@/views/report/CrowdReport.vue";
import EcvpTouristReport from "@/views/report/EcvpTouristReport.vue";
import FenceReport from "@/views/report/FenceReport.vue";
import ParkingReport from "@/views/report/ParkingReport.vue";
import TrafficReport from "@/views/report/TrafficReport.vue";

import { useAuthStore } from "@/stores/auth";

import Login from "@/views/Login.vue";
import Home from "@/views/Home.vue";
import Dashboard from "@/views/dashboard/Dashboard.vue";
import DashboardMap from "@/views/dashboard/DashboardMap.vue";
import Signage from "@/views/Signage.vue";
import ManageAccount from "@/views/ManageAccount.vue";
import ManageDevice from "@/views/ManageDevice.vue";
import ManageDeviceNew from "@/views/ManageDeviceNew.vue";
import ManageStation from "@/views/ManageStation.vue";
import Profile from "@/views/Profile.vue";
import Station from "@/views/Station.vue";
import DevicesStatus from "@/views/DevicesStatus.vue";
import StationDashboard from "@/views/StationDashboard.vue";
import CrowdStreamViewer from "@/views/CrowdStreamViewer.vue";

// 添加全局加載狀態
let isNavigating = false;

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: "/login",
      name: "Login",
      // 2. 使用靜態匯入的元件變數
      component: Login,
      meta: { requiresAuth: false },
    },
    {
      path: "/",
      name: "Home",
      component: Home,
      meta: { requiresAuth: true },
    },
    {
      path: "/dashboard",
      name: "Dashboard",
      component: Dashboard,
      meta: { requiresAuth: true },
    },
    {
      path: "/dashboard-map",
      name: "DashboardMap",
      component: DashboardMap,
      meta: { requiresAuth: true },
    },
    {
      path: "/signage",
      name: "Signage",
      redirect: "/signage/water",
      meta: { requiresAuth: true },
    },
    {
      path: "/signage/water",
      name: "SignageWater",
      component: Signage,
      meta: { requiresAuth: true },
    },
    {
      path: "/signage/bridge",
      name: "SignageBridge",
      component: Signage,
      meta: { requiresAuth: true },
    },
    {
      path: "/signage/digital",
      name: "SignageDigital",
      component: Signage,
      meta: { requiresAuth: true },
    },
    {
      path: "/signage/device",
      name: "SignageDevice",
      component: Signage,
      meta: { requiresAuth: true },
    },
    {
      path: "/home-old",
      name: "HomeOld",
      component: Home,
      meta: { requiresAuth: true },
    },
    {
      path: "/manage-account",
      name: "ManageAccount",
      component: ManageAccount,
      meta: { requiresAuth: true, requiresAdmin: true },
    },
    {
      path: "/manage-device",
      name: "ManageDevice",
      component: ManageDeviceNew,
      meta: { requiresAuth: true },
    },
    {
      path: "/manage-device-old",
      name: "ManageDeviceOld",
      component: ManageDevice,
      meta: { requiresAuth: true },
    },
    {
      path: "/manage-station",
      name: "ManageStation",
      component: ManageStation,
      meta: { requiresAuth: true, requiresAdmin: true },
    },
    {
      path: "/profile",
      name: "Profile",
      component: Profile,
      meta: { requiresAuth: true },
    },
    {
      path: "/stations/:id",
      name: "StationDashboard",
      component: StationDashboard,
      meta: { requiresAuth: true },
    },
    {
      path: "/stations",
      name: "StationDashboardDefault",
      component: StationDashboard,
      meta: { requiresAuth: true },
    },
    {
      path: "/station",
      name: "Station",
      component: Station,
      meta: { requiresAuth: true },
    },
    {
      path: "/devices-status",
      name: "DevicesStatus",
      component: DevicesStatus,
      meta: { requiresAuth: true },
    },
    {
      path: "/crowd-stream",
      name: "CrowdStreamViewer",
      component: CrowdStreamViewer,
      meta: { requiresAuth: false }, // Allow access without auth for new window
    },

    // Overview routes (總覽)
    {
      path: "/overview/crowd",
      name: "OverviewCrowd",
      component: CrowdOverview,
      meta: {
        requiresAuth: true,
        title: "人群總覽",
      },
    },
    {
      path: "/overview/fence",
      name: "OverviewFence",
      component: FenceOverview,
      meta: {
        requiresAuth: true,
        title: "圍欄總覽",
      },
    },
    {
      path: "/overview/parking",
      name: "OverviewParking",
      component: ParkingOverview,
      meta: {
        requiresAuth: true,
        title: "停車總覽",
      },
    },
    {
      path: "/overview/traffic",
      name: "OverviewTraffic",
      component: TrafficOverview,
      meta: {
        requiresAuth: true,
        title: "交通總覽",
      },
    },

    // Report routes (報表)
    {
      path: "/report/crowd",
      name: "ReportCrowd",
      component: CrowdReport,
      meta: {
        requiresAuth: true,
        title: "人流紀錄列表與報表下載",
      },
    },
    {
      path: "/report/fence",
      name: "ReportFence",
      component: FenceReport,
      meta: {
        requiresAuth: true,
        title: "電子圍籬事件報表",
      },
    },
    {
      path: "/report/parking",
      name: "ReportParking",
      component: ParkingReport,
      meta: {
        requiresAuth: true,
        title: "停車場紀錄列表與報表下載",
      },
    },
    {
      path: "/report/traffic",
      name: "ReportTraffic",
      component: TrafficReport,
      meta: {
        requiresAuth: true,
        title: "車流紀錄列表與報表下載",
      },
    },
    {
      path: "/report/admission",
      name: "ReportAdmission",
      component: AdmissionReport,
      meta: {
        requiresAuth: true,
        title: "無接觸入場人次紀錄列表與報表下載",
      },
    },
    {
      path: "/report/ecvp-tourist",
      name: "ReportEcvpTourist",
      component: EcvpTouristReport,
      meta: {
        requiresAuth: true,
        title: "客群輪廓與報表下載",
      },
    },
  ],
});

router.beforeEach(async (to, from, next) => {
  // 防止重複導航
  if (isNavigating) {
    return;
  }

  if (to.meta.title) {
    document.title = to.meta.title;
  }

  const authStore = useAuthStore();

  // 初始化認證狀態
  const localToken = localStorage.getItem("token");
  const sessionToken = sessionStorage.getItem("token");
  if (localToken || sessionToken) {
    authStore.initializeAuth();
  }

  if (to.meta.requiresAuth && !authStore.isAuthenticated) {
    if (to.path === "/") {
      next("/login");
    } else {
      next(`/login?redirect=${to.path}`);
    }
  } else if (to.path === "/login" && authStore.isAuthenticated) {
    next((to.query.redirect as string) || "/");
  } else if (to.meta.requiresAdmin) {
    // 檢查 Admin 權限
    if (!authStore.user) {
      // 如果還沒有使用者資料，嘗試獲取
      try {
        await authStore.getProfile();
      } catch (error) {
        console.error("Failed to get user profile:", error);
        next(`/login?redirect=${to.path}`);
        return;
      }
    }

    if (authStore.user?.role !== "Admin") {
      // 非 Admin 用戶重導向到首頁並顯示錯誤
      console.warn("Access denied: Admin role required");
      next("/?error=access_denied");
      return;
    }

    next();
  } else {
    next();
  }
});

router.beforeResolve((to, from, next) => {
  isNavigating = true;
  next();
});

router.afterEach(() => {
  // 立即重置導航狀態，減少延遲
  isNavigating = false;
});

export default router;
