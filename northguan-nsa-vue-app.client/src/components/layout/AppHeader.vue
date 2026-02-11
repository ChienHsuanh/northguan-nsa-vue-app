<template>
  <header
    class="sticky top-0 z-50 w-full border-b border-border/40 bg-background/95 backdrop-blur supports-[backdrop-filter]:bg-background/60">
    <div class="container mx-auto flex h-16 md:h-14 max-w-screen-2xl items-center justify-between px-4 md:px-6">
      <!-- Left Section: Logo + Navigation -->
      <div class="flex items-center space-x-3 md:space-x-4">
        <!-- Mobile Menu Button -->
        <Button variant="ghost" size="icon"
          class="mobile-menu-trigger md:hidden h-10 w-10 rounded-lg hover:bg-accent/80 transition-colors"
          @click="toggleMobileMenu">
          <MenuIcon class="h-5 w-5" />
          <span class="sr-only">Toggle Menu</span>
        </Button>

        <!-- Logo and Title -->
        <router-link to="/" class="flex items-center space-x-3">
          <div
            class="flex h-9 w-9 md:h-8 md:w-8 items-center justify-center rounded-lg bg-gradient-to-br from-primary to-primary/80 shadow-sm">
            <span class="text-sm font-bold text-primary-foreground">智</span>
          </div>
          <span
            class="hidden font-bold text-lg md:text-base sm:inline-block bg-gradient-to-r from-foreground to-foreground/80 bg-clip-text">
            北觀智慧管理平台
          </span>
        </router-link>

        <!-- Desktop Navigation Menu -->
        <NavigationMenu class="hidden md:flex">
          <NavigationMenuList>
            <!-- 設備即時監控 -->
            <NavigationMenuItem>
              <NavigationMenuLink as-child>
                <router-link to="/"
                  class="group inline-flex h-10 w-max items-center justify-center rounded-md bg-background px-4 py-2 text-sm font-medium transition-colors hover:bg-accent hover:text-accent-foreground focus:bg-accent focus:text-accent-foreground focus:outline-none disabled:pointer-events-none disabled:opacity-50 data-[active]:bg-accent/50 data-[state=open]:bg-accent/50"
                  :class="isHomeActive ? 'bg-accent/50' : ''">
                  設備即時監控
                </router-link>
              </NavigationMenuLink>
            </NavigationMenuItem>

            <!-- 總覽 -->
            <NavigationMenuItem>
              <NavigationMenuTrigger :class="isOverviewActive ? 'bg-accent/50' : ''">
                總覽
              </NavigationMenuTrigger>
              <NavigationMenuContent>
                <div class="grid gap-3 p-6 md:w-[400px] lg:w-[500px] lg:grid-cols-[.75fr_1fr]">
                  <div class="row-span-3">
                    <NavigationMenuLink as-child>
                      <router-link
                        class="flex h-full w-full select-none flex-col justify-end rounded-md bg-gradient-to-b from-muted/50 to-muted p-6 no-underline outline-none focus:shadow-md"
                        to="/overview/crowd">
                        <UsersIcon class="h-6 w-6" />
                        <div class="mb-2 mt-4 text-lg font-medium">
                          人流總覽
                        </div>
                        <p class="text-sm leading-tight text-muted-foreground">
                          查看即時人流狀況和統計數據
                        </p>
                      </router-link>
                    </NavigationMenuLink>
                  </div>
                  <NavigationMenuLink as-child>
                    <router-link to="/overview/fence"
                      class="block select-none space-y-1 rounded-md p-3 leading-none no-underline outline-none transition-colors hover:bg-accent hover:text-accent-foreground focus:bg-accent focus:text-accent-foreground">
                      <div class="text-sm font-medium leading-none">圍籬總覽</div>
                      <p class="line-clamp-2 text-sm leading-snug text-muted-foreground">
                        監控圍籬狀態和安全事件
                      </p>
                    </router-link>
                  </NavigationMenuLink>
                  <NavigationMenuLink as-child>
                    <router-link to="/overview/parking"
                      class="block select-none space-y-1 rounded-md p-3 leading-none no-underline outline-none transition-colors hover:bg-accent hover:text-accent-foreground focus:bg-accent focus:text-accent-foreground">
                      <div class="text-sm font-medium leading-none">停車總覽</div>
                      <p class="line-clamp-2 text-sm leading-snug text-muted-foreground">
                        查看停車場使用情況
                      </p>
                    </router-link>
                  </NavigationMenuLink>
                  <NavigationMenuLink as-child>
                    <router-link to="/overview/traffic"
                      class="block select-none space-y-1 rounded-md p-3 leading-none no-underline outline-none transition-colors hover:bg-accent hover:text-accent-foreground focus:bg-accent focus:text-accent-foreground">
                      <div class="text-sm font-medium leading-none">交通總覽</div>
                      <p class="line-clamp-2 text-sm leading-snug text-muted-foreground">
                        監控交通流量和狀況
                      </p>
                    </router-link>
                  </NavigationMenuLink>
                </div>
              </NavigationMenuContent>
            </NavigationMenuItem>

            <!-- 統計報表 -->
            <NavigationMenuItem>
              <NavigationMenuTrigger :class="isReportActive ? 'bg-accent/50' : ''">
                統計報表
              </NavigationMenuTrigger>
              <NavigationMenuContent>
                <div class="grid w-[400px] gap-3 p-4 md:w-[500px] md:grid-cols-2 lg:w-[600px]">
                  <NavigationMenuLink as-child>
                    <router-link to="/report/crowd"
                      class="block select-none space-y-1 rounded-md p-3 leading-none no-underline outline-none transition-colors hover:bg-accent hover:text-accent-foreground focus:bg-accent focus:text-accent-foreground">
                      <div class="text-sm font-medium leading-none">人流報表</div>
                      <p class="line-clamp-2 text-sm leading-snug text-muted-foreground">
                        人流統計和分析報表
                      </p>
                    </router-link>
                  </NavigationMenuLink>
                  <NavigationMenuLink as-child>
                    <router-link to="/report/fence"
                      class="block select-none space-y-1 rounded-md p-3 leading-none no-underline outline-none transition-colors hover:bg-accent hover:text-accent-foreground focus:bg-accent focus:text-accent-foreground">
                      <div class="text-sm font-medium leading-none">圍籬報表</div>
                      <p class="line-clamp-2 text-sm leading-snug text-muted-foreground">
                        圍籬事件統計報表
                      </p>
                    </router-link>
                  </NavigationMenuLink>
                  <NavigationMenuLink as-child>
                    <router-link to="/report/parking"
                      class="block select-none space-y-1 rounded-md p-3 leading-none no-underline outline-none transition-colors hover:bg-accent hover:text-accent-foreground focus:bg-accent focus:text-accent-foreground">
                      <div class="text-sm font-medium leading-none">停車報表</div>
                      <p class="line-clamp-2 text-sm leading-snug text-muted-foreground">
                        停車場使用統計
                      </p>
                    </router-link>
                  </NavigationMenuLink>
                  <NavigationMenuLink as-child>
                    <router-link to="/report/traffic"
                      class="block select-none space-y-1 rounded-md p-3 leading-none no-underline outline-none transition-colors hover:bg-accent hover:text-accent-foreground focus:bg-accent focus:text-accent-foreground">
                      <div class="text-sm font-medium leading-none">交通報表</div>
                      <p class="line-clamp-2 text-sm leading-snug text-muted-foreground">
                        交通流量統計報表
                      </p>
                    </router-link>
                  </NavigationMenuLink>
                  <!-- <NavigationMenuLink as-child>
                    <router-link
                      to="/report/admission"
                      class="block select-none space-y-1 rounded-md p-3 leading-none no-underline outline-none transition-colors hover:bg-accent hover:text-accent-foreground focus:bg-accent focus:text-accent-foreground"
                    >
                      <div class="text-sm font-medium leading-none">入園報表</div>
                      <p class="line-clamp-2 text-sm leading-snug text-muted-foreground">
                        入園人數統計分析
                      </p>
                    </router-link>
                  </NavigationMenuLink>
                  <NavigationMenuLink as-child>
                    <router-link
                      to="/report/ecvp-tourist"
                      class="block select-none space-y-1 rounded-md p-3 leading-none no-underline outline-none transition-colors hover:bg-accent hover:text-accent-foreground focus:bg-accent focus:text-accent-foreground"
                    >
                      <div class="text-sm font-medium leading-none">ECVP遊客報表</div>
                      <p class="line-clamp-2 text-sm leading-snug text-muted-foreground">
                        ECVP遊客數據統計
                      </p>
                    </router-link>
                  </NavigationMenuLink> -->
                </div>
              </NavigationMenuContent>
            </NavigationMenuItem>

            <!-- 系統管理 -->
            <NavigationMenuItem>
              <NavigationMenuTrigger :class="isManageActive ? 'bg-accent/50' : ''">
                系統管理
              </NavigationMenuTrigger>
              <NavigationMenuContent>
                <div class="grid gap-3 p-6 md:w-[400px] lg:w-[500px]"
                  :class="isAdmin ? 'lg:grid-cols-[.75fr_1fr]' : 'lg:grid-cols-1'">
                  <!-- 帳號管理 - 僅管理員可見 -->
                  <div v-if="isAdmin" class="row-span-3">
                    <NavigationMenuLink as-child>
                      <router-link
                        class="flex h-full w-full select-none flex-col justify-end rounded-md bg-gradient-to-b from-muted/50 to-muted p-6 no-underline outline-none focus:shadow-md"
                        to="/manage-account">
                        <UserIcon class="h-6 w-6" />
                        <div class="mb-2 mt-4 text-lg font-medium">
                          帳號管理
                        </div>
                        <p class="text-sm leading-tight text-muted-foreground">
                          管理使用者帳號和權限
                        </p>
                      </router-link>
                    </NavigationMenuLink>
                  </div>
                  <!-- 設備管理 - 所有用戶可見 -->
                  <NavigationMenuLink as-child>
                    <router-link to="/manage-device"
                      class="block select-none space-y-1 rounded-md p-3 leading-none no-underline outline-none transition-colors hover:bg-accent hover:text-accent-foreground focus:bg-accent focus:text-accent-foreground">
                      <div class="text-sm font-medium leading-none">設備管理</div>
                      <p class="line-clamp-2 text-sm leading-snug text-muted-foreground">
                        管理監控設備和感測器
                      </p>
                    </router-link>
                  </NavigationMenuLink>
                  <!-- 分站管理 - 僅管理員可見 -->
                  <NavigationMenuLink v-if="isAdmin" as-child>
                    <router-link to="/manage-station"
                      class="block select-none space-y-1 rounded-md p-3 leading-none no-underline outline-none transition-colors hover:bg-accent hover:text-accent-foreground focus:bg-accent focus:text-accent-foreground">
                      <div class="text-sm font-medium leading-none">分站管理</div>
                      <p class="line-clamp-2 text-sm leading-snug text-muted-foreground">
                        管理各分站設定和資訊
                      </p>
                    </router-link>
                  </NavigationMenuLink>
                </div>
              </NavigationMenuContent>
            </NavigationMenuItem>

            <!-- 戰情室總覽 -->
            <NavigationMenuItem>
              <NavigationMenuLink as-child>
                <router-link to="/dashboard"
                  class="group inline-flex h-10 w-max items-center justify-center rounded-md bg-background px-4 py-2 text-sm font-medium transition-colors hover:bg-accent hover:text-accent-foreground focus:bg-accent focus:text-accent-foreground focus:outline-none disabled:pointer-events-none disabled:opacity-50 data-[active]:bg-accent/50 data-[state=open]:bg-accent/50">
                  戰情室總覽
                </router-link>
              </NavigationMenuLink>
            </NavigationMenuItem>
          </NavigationMenuList>
        </NavigationMenu>
      </div>

      <!-- Right Section: Search + Notifications + User Menu -->
      <div class="flex items-center space-x-2 md:space-x-3">
        <!-- Mobile Search Button -->
        <Button variant="ghost" size="icon" class="md:hidden h-10 w-10 rounded-lg hover:bg-accent/80 transition-colors"
          @click="toggleMobileSearch">
          <SearchIcon class="h-5 w-5" />
          <span class="sr-only">搜尋</span>
        </Button>

        <!-- Desktop Search -->
        <!-- <div class="relative hidden lg:block">
          <SearchIcon class="absolute left-3 top-1/2 h-4 w-4 text-muted-foreground transform -translate-y-1/2" />
          <Input
            type="search"
            placeholder="搜尋..."
            class="w-[220px] pl-10 h-9 rounded-lg border-border/50 focus:border-primary/50 transition-colors"
            v-model="searchQuery"
            @keyup.enter="handleSearch"
          />
        </div> -->

        <!-- Notifications -->
        <!-- <Button variant="ghost" size="icon" class="relative h-10 w-10 md:h-8 md:w-8 rounded-lg hover:bg-accent/80 transition-colors">
          <BellIcon class="h-5 w-5 md:h-4 md:w-4" />
          <span class="absolute -top-1 -right-1 h-4 w-4 md:h-3 md:w-3 rounded-full bg-gradient-to-r from-red-500 to-red-600 text-[10px] font-medium text-white flex items-center justify-center shadow-sm">
            3
          </span>
          <span class="sr-only">通知</span>
        </Button> -->

        <!-- User Menu -->
        <DropdownMenu>
          <DropdownMenuTrigger as-child>
            <Button variant="ghost"
              class="relative h-10 w-10 md:h-8 md:w-8 rounded-full hover:bg-accent/80 transition-colors">
              <Avatar class="h-10 w-10 md:h-8 md:w-8 ring-2 ring-border/20">
                <AvatarImage :src="user?.avatar ?? ''" :alt="user?.name || '使用者'" />
                <AvatarFallback class="bg-gradient-to-br from-primary/20 to-primary/10 text-primary font-semibold">{{
                  getUserInitials() }}</AvatarFallback>
              </Avatar>
            </Button>
          </DropdownMenuTrigger>
          <DropdownMenuContent class="w-56" align="end" :side-offset="4">
            <DropdownMenuLabel class="font-normal">
              <div class="flex flex-col space-y-1">
                <p class="text-sm font-medium leading-none">{{ user?.name || '使用者' }}</p>
                <p class="text-xs leading-none text-muted-foreground">
                  {{ user?.email || 'user@example.com' }}
                </p>
              </div>
            </DropdownMenuLabel>
            <DropdownMenuSeparator />
            <DropdownMenuGroup>
              <DropdownMenuItem as-child>
                <router-link to="/profile" class="flex items-center">
                  <UserIcon class="mr-2 h-4 w-4" />
                  <span>個人資料</span>
                </router-link>
              </DropdownMenuItem>
              <!-- <DropdownMenuItem>
                <SettingsIcon class="mr-2 h-4 w-4" />
                <span>設定</span>
              </DropdownMenuItem> -->
            </DropdownMenuGroup>
            <DropdownMenuSeparator />
            <DropdownMenuItem @click="logout">
              <LogOutIcon class="mr-2 h-4 w-4" />
              <span>登出</span>
            </DropdownMenuItem>
          </DropdownMenuContent>
        </DropdownMenu>
      </div>
    </div>
  </header>

  <!-- Mobile Navigation -->
  <div v-if="showMobileMenu" class="fixed inset-0 top-16 z-40 md:hidden mobile-menu mobile-menu-enter"
    @click="closeMobileMenu">
    <!-- Backdrop -->
    <div class="absolute inset-0 bg-black/20 backdrop-blur-sm mobile-backdrop-enter"></div>

    <!-- Menu Content -->
    <div
      class="absolute inset-0 bg-background/98 backdrop-blur-md border-t border-border/50 overflow-hidden flex flex-col mobile-content-enter"
      @click.stop>
      <!-- Close Button -->
      <div class="flex justify-end p-4 border-b border-border/30 flex-shrink-0">
        <Button variant="ghost" size="icon" class="h-10 w-10 rounded-lg hover:bg-accent/80 transition-colors"
          @click="closeMobileMenu">
          <XIcon class="h-5 w-5" />
          <span class="sr-only">關閉選單</span>
        </Button>
      </div>

      <!-- Mobile Search Bar -->
      <div v-if="showMobileSearch" class="p-4 border-b border-border/30 flex-shrink-0">
        <div class="relative">
          <SearchIcon class="absolute left-3 top-1/2 h-4 w-4 text-muted-foreground transform -translate-y-1/2" />
          <Input type="search" placeholder="搜尋功能或頁面..."
            class="w-full pl-10 h-11 rounded-xl border-border/50 focus:border-primary/50 transition-all duration-200"
            v-model="searchQuery" @keyup.enter="handleSearch" />
        </div>
      </div>

      <nav class="flex-1 overflow-y-auto min-h-0">
        <!-- 首頁 -->
        <div class="p-4 pb-2">
          <router-link to="/"
            class="flex items-center space-x-4 rounded-xl px-4 py-3 text-base font-medium hover:bg-gradient-to-r hover:from-accent/80 hover:to-accent/60 transition-all duration-200 group"
            :class="isHomeActive ? 'bg-gradient-to-r from-primary/10 to-primary/5 text-primary border border-primary/20' : ''"
            @click="closeMobileMenu">
            <div
              class="flex h-10 w-10 items-center justify-center rounded-lg bg-gradient-to-br from-blue-500/20 to-blue-600/10 group-hover:from-blue-500/30 group-hover:to-blue-600/20 transition-all duration-200">
              <HomeIcon class="h-5 w-5 text-blue-600" />
            </div>
            <span>首頁</span>
          </router-link>
        </div>

        <!-- 總覽 -->
        <div class="px-4 pb-2">
          <div class="flex items-center space-x-2 px-4 py-2 mb-2">
            <div class="h-1 w-1 rounded-full bg-primary"></div>
            <span class="text-sm font-semibold text-muted-foreground">總覽</span>
            <div class="flex-1 h-px bg-border/30"></div>
          </div>
          <div class="space-y-1">
            <router-link to="/overview/crowd"
              class="flex items-center space-x-4 rounded-xl px-4 py-3 text-sm font-medium hover:bg-gradient-to-r hover:from-accent/80 hover:to-accent/60 transition-all duration-200 group"
              :class="route.path === '/overview/crowd' ? 'bg-gradient-to-r from-primary/10 to-primary/5 text-primary border border-primary/20' : ''"
              @click="closeMobileMenu">
              <div
                class="flex h-9 w-9 items-center justify-center rounded-lg bg-gradient-to-br from-green-500/20 to-green-600/10 group-hover:from-green-500/30 group-hover:to-green-600/20 transition-all duration-200">
                <UsersIcon class="h-4 w-4 text-green-600" />
              </div>
              <span>人流總覽</span>
            </router-link>
            <router-link to="/overview/fence"
              class="flex items-center space-x-4 rounded-xl px-4 py-3 text-sm font-medium hover:bg-gradient-to-r hover:from-accent/80 hover:to-accent/60 transition-all duration-200 group"
              :class="route.path === '/overview/fence' ? 'bg-gradient-to-r from-primary/10 to-primary/5 text-primary border border-primary/20' : ''"
              @click="closeMobileMenu">
              <div
                class="flex h-9 w-9 items-center justify-center rounded-lg bg-gradient-to-br from-orange-500/20 to-orange-600/10 group-hover:from-orange-500/30 group-hover:to-orange-600/20 transition-all duration-200">
                <ShieldIcon class="h-4 w-4 text-orange-600" />
              </div>
              <span>圍籬總覽</span>
            </router-link>
            <router-link to="/overview/parking"
              class="flex items-center space-x-4 rounded-xl px-4 py-3 text-sm font-medium hover:bg-gradient-to-r hover:from-accent/80 hover:to-accent/60 transition-all duration-200 group"
              :class="route.path === '/overview/parking' ? 'bg-gradient-to-r from-primary/10 to-primary/5 text-primary border border-primary/20' : ''"
              @click="closeMobileMenu">
              <div
                class="flex h-9 w-9 items-center justify-center rounded-lg bg-gradient-to-br from-purple-500/20 to-purple-600/10 group-hover:from-purple-500/30 group-hover:to-purple-600/20 transition-all duration-200">
                <CarIcon class="h-4 w-4 text-purple-600" />
              </div>
              <span>停車總覽</span>
            </router-link>
            <router-link to="/overview/traffic"
              class="flex items-center space-x-4 rounded-xl px-4 py-3 text-sm font-medium hover:bg-gradient-to-r hover:from-accent/80 hover:to-accent/60 transition-all duration-200 group"
              :class="route.path === '/overview/traffic' ? 'bg-gradient-to-r from-primary/10 to-primary/5 text-primary border border-primary/20' : ''"
              @click="closeMobileMenu">
              <div
                class="flex h-9 w-9 items-center justify-center rounded-lg bg-gradient-to-br from-red-500/20 to-red-600/10 group-hover:from-red-500/30 group-hover:to-red-600/20 transition-all duration-200">
                <TrafficConeIcon class="h-4 w-4 text-red-600" />
              </div>
              <span>交通總覽</span>
            </router-link>
          </div>
        </div>

        <!-- 統計報表 -->
        <div class="px-4 pb-2">
          <div class="flex items-center space-x-2 px-4 py-2 mb-2">
            <div class="h-1 w-1 rounded-full bg-primary"></div>
            <span class="text-sm font-semibold text-muted-foreground">統計報表</span>
            <div class="flex-1 h-px bg-border/30"></div>
          </div>
          <div class="space-y-1">
            <router-link to="/report/crowd"
              class="flex items-center space-x-4 rounded-xl px-4 py-3 text-sm font-medium hover:bg-gradient-to-r hover:from-accent/80 hover:to-accent/60 transition-all duration-200 group"
              :class="route.path === '/report/crowd' ? 'bg-gradient-to-r from-primary/10 to-primary/5 text-primary border border-primary/20' : ''"
              @click="closeMobileMenu">
              <div
                class="flex h-9 w-9 items-center justify-center rounded-lg bg-gradient-to-br from-indigo-500/20 to-indigo-600/10 group-hover:from-indigo-500/30 group-hover:to-indigo-600/20 transition-all duration-200">
                <BarChart3Icon class="h-4 w-4 text-indigo-600" />
              </div>
              <span>人流報表</span>
            </router-link>
            <router-link to="/report/fence"
              class="flex items-center space-x-4 rounded-xl px-4 py-3 text-sm font-medium hover:bg-gradient-to-r hover:from-accent/80 hover:to-accent/60 transition-all duration-200 group"
              :class="route.path === '/report/fence' ? 'bg-gradient-to-r from-primary/10 to-primary/5 text-primary border border-primary/20' : ''"
              @click="closeMobileMenu">
              <div
                class="flex h-9 w-9 items-center justify-center rounded-lg bg-gradient-to-br from-indigo-500/20 to-indigo-600/10 group-hover:from-indigo-500/30 group-hover:to-indigo-600/20 transition-all duration-200">
                <BarChart3Icon class="h-4 w-4 text-indigo-600" />
              </div>
              <span>圍籬報表</span>
            </router-link>
            <router-link to="/report/parking"
              class="flex items-center space-x-4 rounded-xl px-4 py-3 text-sm font-medium hover:bg-gradient-to-r hover:from-accent/80 hover:to-accent/60 transition-all duration-200 group"
              :class="route.path === '/report/parking' ? 'bg-gradient-to-r from-primary/10 to-primary/5 text-primary border border-primary/20' : ''"
              @click="closeMobileMenu">
              <div
                class="flex h-9 w-9 items-center justify-center rounded-lg bg-gradient-to-br from-indigo-500/20 to-indigo-600/10 group-hover:from-indigo-500/30 group-hover:to-indigo-600/20 transition-all duration-200">
                <BarChart3Icon class="h-4 w-4 text-indigo-600" />
              </div>
              <span>停車報表</span>
            </router-link>
            <router-link to="/report/traffic"
              class="flex items-center space-x-4 rounded-xl px-4 py-3 text-sm font-medium hover:bg-gradient-to-r hover:from-accent/80 hover:to-accent/60 transition-all duration-200 group"
              :class="route.path === '/report/traffic' ? 'bg-gradient-to-r from-primary/10 to-primary/5 text-primary border border-primary/20' : ''"
              @click="closeMobileMenu">
              <div
                class="flex h-9 w-9 items-center justify-center rounded-lg bg-gradient-to-br from-indigo-500/20 to-indigo-600/10 group-hover:from-indigo-500/30 group-hover:to-indigo-600/20 transition-all duration-200">
                <BarChart3Icon class="h-4 w-4 text-indigo-600" />
              </div>
              <span>交通報表</span>
            </router-link>
          </div>
        </div>

        <!-- 系統管理 -->
        <div class="px-4 pb-4">
          <div class="flex items-center space-x-2 px-4 py-2 mb-2">
            <div class="h-1 w-1 rounded-full bg-primary"></div>
            <span class="text-sm font-semibold text-muted-foreground">系統管理</span>
            <div class="flex-1 h-px bg-border/30"></div>
          </div>
          <div class="space-y-1">
            <!-- 帳號管理 - 僅管理員可見 -->
            <router-link v-if="isAdmin" to="/manage-account"
              class="flex items-center space-x-4 rounded-xl px-4 py-3 text-sm font-medium hover:bg-gradient-to-r hover:from-accent/80 hover:to-accent/60 transition-all duration-200 group"
              :class="route.path === '/manage-account' ? 'bg-gradient-to-r from-primary/10 to-primary/5 text-primary border border-primary/20' : ''"
              @click="closeMobileMenu">
              <div
                class="flex h-9 w-9 items-center justify-center rounded-lg bg-gradient-to-br from-teal-500/20 to-teal-600/10 group-hover:from-teal-500/30 group-hover:to-teal-600/20 transition-all duration-200">
                <UserIcon class="h-4 w-4 text-teal-600" />
              </div>
              <span>帳號管理</span>
            </router-link>
            <!-- 設備管理 - 所有用戶可見 -->
            <router-link to="/manage-device"
              class="flex items-center space-x-4 rounded-xl px-4 py-3 text-sm font-medium hover:bg-gradient-to-r hover:from-accent/80 hover:to-accent/60 transition-all duration-200 group"
              :class="route.path === '/manage-device' ? 'bg-gradient-to-r from-primary/10 to-primary/5 text-primary border border-primary/20' : ''"
              @click="closeMobileMenu">
              <div
                class="flex h-9 w-9 items-center justify-center rounded-lg bg-gradient-to-br from-slate-500/20 to-slate-600/10 group-hover:from-slate-500/30 group-hover:to-slate-600/20 transition-all duration-200">
                <SettingsIcon class="h-4 w-4 text-slate-600" />
              </div>
              <span>設備管理</span>
            </router-link>
            <!-- 分站管理 - 僅管理員可見 -->
            <router-link v-if="isAdmin" to="/manage-station"
              class="flex items-center space-x-4 rounded-xl px-4 py-3 text-sm font-medium hover:bg-gradient-to-r hover:from-accent/80 hover:to-accent/60 transition-all duration-200 group"
              :class="route.path === '/manage-station' ? 'bg-gradient-to-r from-primary/10 to-primary/5 text-primary border border-primary/20' : ''"
              @click="closeMobileMenu">
              <div
                class="flex h-9 w-9 items-center justify-center rounded-lg bg-gradient-to-br from-slate-500/20 to-slate-600/10 group-hover:from-slate-500/30 group-hover:to-slate-600/20 transition-all duration-200">
                <SettingsIcon class="h-4 w-4 text-slate-600" />
              </div>
              <span>分站管理</span>
            </router-link>
          </div>
        </div>

        <!-- 戰情室總覽 -->
        <div class="p-4 pb-2">
          <router-link to="/dashboard"
            class="flex items-center space-x-4 rounded-xl px-4 py-3 text-base font-medium hover:bg-gradient-to-r hover:from-accent/80 hover:to-accent/60 transition-all duration-200 group"
            @click="closeMobileMenu">
            <div
              class="flex h-10 w-10 items-center justify-center rounded-lg bg-gradient-to-br from-blue-500/20 to-blue-600/10 group-hover:from-blue-500/30 group-hover:to-blue-600/20 transition-all duration-200">
              <LayoutDashboardIcon class="h-5 w-5 text-blue-600" />
            </div>
            <span>戰情室總覽</span>
          </router-link>
        </div>
      </nav>
    </div>
  </div>
</template>

<style scoped>
/* 移動端選單動畫 */
.mobile-menu-enter {
  animation: slideIn 0.3s ease-out;
}

.mobile-backdrop-enter {
  animation: fadeIn 0.3s ease-out;
}

.mobile-content-enter {
  animation: slideUp 0.4s cubic-bezier(0.25, 0.46, 0.45, 0.94);
}

@keyframes slideIn {
  from {
    opacity: 0;
  }

  to {
    opacity: 1;
  }
}

@keyframes fadeIn {
  from {
    opacity: 0;
  }

  to {
    opacity: 1;
  }
}

@keyframes slideUp {
  from {
    transform: translateY(100%);
    opacity: 0.8;
  }

  to {
    transform: translateY(0);
    opacity: 1;
  }
}

/* 為移動端菜單項目添加交錯動畫 */
.mobile-menu nav>div {
  animation: slideInItem 0.5s ease-out forwards;
  opacity: 0;
  transform: translateX(-20px);
}

.mobile-menu nav>div:nth-child(1) {
  animation-delay: 0.1s;
}

.mobile-menu nav>div:nth-child(2) {
  animation-delay: 0.15s;
}

.mobile-menu nav>div:nth-child(3) {
  animation-delay: 0.2s;
}

.mobile-menu nav>div:nth-child(4) {
  animation-delay: 0.25s;
}

@keyframes slideInItem {
  from {
    opacity: 0;
    transform: translateX(-20px);
  }

  to {
    opacity: 1;
    transform: translateX(0);
  }
}

/* 增強按鈕點擊效果 */
.mobile-menu .group:active {
  transform: scale(0.98);
  transition: transform 0.1s ease-in-out;
}

/* 按鈕懸停效果增強 */
.mobile-menu .group {
  transition: all 0.2s ease-in-out;
}

.mobile-menu .group:hover {
  transform: translateX(4px);
}

/* 圖標容器動畫 */
.mobile-menu .group>div:first-child {
  transition: all 0.3s ease-in-out;
}

/* 關閉按鈕動畫 */
.mobile-menu button {
  transition: all 0.2s ease-in-out;
}

/* 搜索框動畫 */
.mobile-menu input {
  transition: all 0.3s ease-in-out;
}

.mobile-menu input:focus {
  transform: scale(1.02);
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
}

/* 滾動條美化 */
.mobile-menu nav::-webkit-scrollbar {
  width: 4px;
}

.mobile-menu nav::-webkit-scrollbar-track {
  background: transparent;
}

.mobile-menu nav::-webkit-scrollbar-thumb {
  background: rgba(0, 0, 0, 0.1);
  border-radius: 2px;
  transition: background 0.2s ease-in-out;
}

.mobile-menu nav::-webkit-scrollbar-thumb:hover {
  background: rgba(0, 0, 0, 0.2);
}

/* 分隔線動畫 */
.mobile-menu .h-px {
  transition: all 0.3s ease-in-out;
  transform-origin: left;
}

.mobile-menu:hover .h-px {
  transform: scaleX(1.1);
  background: linear-gradient(90deg, transparent, currentColor, transparent);
}

/* 響應式動畫優化 */
@media (prefers-reduced-motion: reduce) {

  .mobile-menu-enter,
  .mobile-backdrop-enter,
  .mobile-content-enter,
  .mobile-menu nav>div {
    animation: none;
  }

  .mobile-menu .group,
  .mobile-menu .group>div:first-child,
  .mobile-menu button,
  .mobile-menu input {
    transition: none;
  }
}
</style>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import Button from '@/components/ui/button/Button.vue'
import Input from '@/components/ui/input/Input.vue'
import {
  NavigationMenu,
  NavigationMenuContent,
  NavigationMenuItem,
  NavigationMenuLink,
  NavigationMenuList,
  NavigationMenuTrigger,
} from '@/components/ui/navigation-menu'
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuGroup,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu'
import {
  Avatar,
  AvatarFallback,
  AvatarImage,
} from '@/components/ui/avatar'
import {
  Menu as MenuIcon,
  Search as SearchIcon,
  User as UserIcon,
  Settings as SettingsIcon,
  LogOut as LogOutIcon,
  Home as HomeIcon,
  Users as UsersIcon,
  BarChart3 as BarChart3Icon,
  Shield as ShieldIcon,
  Car as CarIcon,
  TrafficCone as TrafficConeIcon,
  X as XIcon,
  LayoutDashboard as LayoutDashboardIcon,
} from 'lucide-vue-next'

const router = useRouter()
const route = useRoute()
const authStore = useAuthStore()

const showMobileMenu = ref(false)
const showMobileSearch = ref(false)
const searchQuery = ref('')
const user = computed(() => authStore.user)

// 檢查是否為管理員權限
const isAdmin = computed(() => authStore.user?.role === 'Admin')

// 緩存路由狀態檢查，減少重複計算
const isOverviewActive = computed(() => route.path.startsWith('/overview'))
const isReportActive = computed(() => route.path.startsWith('/report'))
const isManageActive = computed(() => route.path.startsWith('/manage'))
const isHomeActive = computed(() => route.path === '/')

const toggleMobileMenu = () => {
  showMobileMenu.value = !showMobileMenu.value
  // 關閉搜尋當開啟選單時
  if (showMobileMenu.value) {
    showMobileSearch.value = false
  }
}

const toggleMobileSearch = () => {
  showMobileSearch.value = !showMobileSearch.value
  // 確保選單是開啟的當顯示搜尋時
  if (showMobileSearch.value && !showMobileMenu.value) {
    showMobileMenu.value = true
  }
}

const closeMobileMenu = () => {
  showMobileMenu.value = false
  showMobileSearch.value = false
}

const handleSearch = () => {
  if (searchQuery.value.trim()) {
    console.log('搜尋:', searchQuery.value)
    // 可以導航到搜尋結果頁面或執行搜尋邏輯
  }
}

const getUserInitials = () => {
  const name = user.value?.name || '使用者'
  return name.charAt(0).toUpperCase()
}

const logout = () => {
  authStore.logout()
  router.push('/login')
}

const handleClickOutside = (event: Event) => {
  const target = event.target as HTMLElement
  // 檢查點擊是否在移動端選單、觸發按鈕、header 或下拉選單內
  if (!target.closest('.mobile-menu') &&
    !target.closest('.mobile-menu-trigger') &&
    !target.closest('header') &&
    !target.closest('[data-radix-popper-content-wrapper]') &&
    !target.closest('[role="menu"]') &&
    showMobileMenu.value) {
    closeMobileMenu()
  }
}

onMounted(() => {
  document.addEventListener('click', handleClickOutside)
  // 只在必要時載入使用者資料，避免重複請求
  if (authStore.isAuthenticated && !authStore.user) {
    authStore.getProfile().catch(error => {
      console.error('Failed to load user profile:', error)
    })
  }
})

onUnmounted(() => {
  document.removeEventListener('click', handleClickOutside)
})
</script>
