'use client'

import { Avatar } from '@/components/avatar'
import { useUser } from '@/hooks/useUser'
import {
  Dropdown,
  DropdownButton,
  DropdownDivider,
  DropdownItem,
  DropdownLabel,
  DropdownMenu,
} from '@/components/dropdown'
import { Navbar, NavbarItem, NavbarSection, NavbarSpacer } from '@/components/navbar'
import {
  Sidebar,
  SidebarBody,
  SidebarFooter,
  SidebarHeader,
  SidebarHeading,
  SidebarItem,
  SidebarLabel,
  SidebarSection,
  SidebarSpacer,
} from '@/components/sidebar'
import { SidebarLayout } from '@/components/sidebar-layout'
import { getEvents } from '@/data'
import {
  ArrowRightStartOnRectangleIcon,
  ChevronDownIcon,
  ChevronUpIcon,
  Cog8ToothIcon,
  LightBulbIcon,
  PlusIcon,
  ShieldCheckIcon,
  UserCircleIcon,
} from '@heroicons/react/16/solid'
import {
  Cog6ToothIcon,
  HomeIcon,
  QuestionMarkCircleIcon,
  SparklesIcon,
  Square2StackIcon,
  TicketIcon,
  ExclamationTriangleIcon,
} from '@heroicons/react/20/solid'
import { usePathname, useSearchParams } from 'next/navigation'
import { useRouter } from 'next/navigation'

function clearAuthData() {
  localStorage.removeItem('token')
  localStorage.removeItem('refreshToken')
  document.cookie = 'token=; Max-Age=0; path=/;'
  document.cookie = 'refreshToken=; Max-Age=0; path=/;'
}

function AccountDropdownMenu({ anchor }: { anchor: 'top start' | 'bottom end' }) {
  const router = useRouter()

  const handleSignOut = (e: React.MouseEvent) => {
    e.preventDefault()
    clearAuthData()
    router.push('/login')
  }

  return (
    <DropdownMenu className="min-w-64" anchor={anchor}>
      <DropdownItem href="/account">
        <UserCircleIcon />
        <DropdownLabel>My account</DropdownLabel>
      </DropdownItem>
      <DropdownDivider />
 {/*     <DropdownItem href="#">
        <ShieldCheckIcon />
        <DropdownLabel>Privacy policy</DropdownLabel>
      </DropdownItem>
      <DropdownItem href="#">
        <LightBulbIcon />
        <DropdownLabel>Share feedback</DropdownLabel>
      </DropdownItem>
      <DropdownDivider />*/}
      <DropdownItem href="/login" onClick={handleSignOut}>
        <ArrowRightStartOnRectangleIcon />
        <DropdownLabel>Sign out</DropdownLabel>
      </DropdownItem>
    </DropdownMenu>
  )
}

export function ApplicationLayout({
  events,
  children,
}: {
  events: Awaited<ReturnType<typeof getEvents>>
  children: React.ReactNode
}) {
  const pathname = usePathname()
  const { user } = useUser()
  const searchParams = useSearchParams()
  const hiddenBar = pathname.startsWith('/collections/') && (/\/chat($|\/)/.test(pathname))

  const avatarUrl = user?.avatarFileName
    ? `${process.env.NEXT_PUBLIC_API_URL}/auth/avatar/${user.avatarFileName}`
    : '/users/default.jpg'

  const NavbarContent = (
    <Navbar>
      <NavbarSpacer />
      <NavbarSection>
        <Dropdown>
          <DropdownButton as={NavbarItem}>
            <Avatar src={avatarUrl} square />
          </DropdownButton>
          <AccountDropdownMenu anchor="bottom end" />
        </Dropdown>
      </NavbarSection>
    </Navbar>
  )

  if (hiddenBar) {
    // ✅ Plein écran sans sidebar
    return (
      <div className="flex flex-col min-h-screen w-full">
        <main className="flex-1 w-full">{children}</main>
      </div>
    )
  }

  return (
    <SidebarLayout
      navbar={NavbarContent}
      sidebar={
        <Sidebar>
          <SidebarHeader>
            <Dropdown>
              <DropdownButton as={SidebarItem}>
                <Avatar src="/teams/luxoria.svg" />
                <SidebarLabel>Luxoria</SidebarLabel>
              </DropdownButton>
            </Dropdown>
          </SidebarHeader> 

          <SidebarBody>
            <SidebarSection>
              <SidebarItem href="/" current={pathname === '/'}>
                <HomeIcon />
                <SidebarLabel>Home</SidebarLabel>
              </SidebarItem>
              <SidebarItem href="/collections" current={pathname.startsWith('/collections')}>
                <Square2StackIcon />
                <SidebarLabel>Collections</SidebarLabel>
              </SidebarItem>
    {/*          <SidebarItem href="/settings" current={pathname.startsWith('/settings')}>
                <Cog6ToothIcon />
                <SidebarLabel>Settings</SidebarLabel>
              </SidebarItem> */}
            </SidebarSection>

            {user?.role === 2 && (
              <SidebarSection>
                <SidebarHeading>Admin</SidebarHeading>
                <SidebarItem href="/admin/users">
                  <UserCircleIcon />
                  <SidebarLabel>User Management</SidebarLabel>
                </SidebarItem>
                <SidebarItem href="/admin/collections">
                  <Square2StackIcon />
                  <SidebarLabel>Collections Management</SidebarLabel>
                </SidebarItem>
                <SidebarItem href="/admin/logs">
                  <TicketIcon />
                  <SidebarLabel>Activity Logs</SidebarLabel>
                </SidebarItem>
                <SidebarItem href="/admin/reports">
                  <ExclamationTriangleIcon />
                  <SidebarLabel>View Reports</SidebarLabel>
                </SidebarItem>
                <SidebarItem
                  onClick={() => {
                    if (
                      window.confirm(
                        'You will be redirected to a Google Forms page to request Sentry access. Continue?'
                      )
                    ) {
                      window.open('https://docs.google.com/forms/d/e/1FAIpQLSekFe85AONUMKtMXm0uG_vXh-TknDYMBUqLNLzNliDWIzazeQ/viewform?usp=header', '_blank')
                    }
                  }}
                >
                  <ShieldCheckIcon />
                  <SidebarLabel>Sentry Access</SidebarLabel>
                </SidebarItem>
                <SidebarItem
                  onClick={() => {
                    if (
                      window.confirm(
                        'You will be redirected to a Google Forms page to request Feedback access. Continue?'
                      )
                    ) {
                      window.open('https://docs.google.com/forms/d/e/1FAIpQLSc8AYwfTPP-_Iu_XKXfxwsAnET2-X_MOO7POxFH7dBCJLJfFw/viewform?usp=header', '_blank')
                    }
                  }}
                >
                  <LightBulbIcon />
                  <SidebarLabel>Feedback Access</SidebarLabel>
                </SidebarItem>
              </SidebarSection>
            )}

            <SidebarSpacer />

       {/*     <SidebarSection>
              <SidebarItem href="#">
                <QuestionMarkCircleIcon />
                <SidebarLabel>Support</SidebarLabel>
              </SidebarItem>
              <SidebarItem href="#">
                <SparklesIcon />
                <SidebarLabel>Changelog</SidebarLabel>
              </SidebarItem>
            </SidebarSection> */}
          </SidebarBody>

          <SidebarFooter className="max-lg:hidden">
            <Dropdown>
              <DropdownButton as={SidebarItem}>
                <span className="flex min-w-0 items-center gap-3">
                  <Avatar src={avatarUrl} className="size-10" square alt="" />
                  <span className="min-w-0">
                    <span className="block truncate text-sm/5 font-medium text-zinc-950 dark:text-white">
                      {user?.username || 'Utilisateur'}
                    </span>
                    <span className="block truncate text-xs/5 font-normal text-zinc-500 dark:text-zinc-400">
                      {user?.email || '—'}
                    </span>
                  </span>
                </span>
                <ChevronUpIcon />
              </DropdownButton>
              <AccountDropdownMenu anchor="top start" />
            </Dropdown>
          </SidebarFooter>
        </Sidebar>
      }
    >
      {children}
    </SidebarLayout>
  )
}
