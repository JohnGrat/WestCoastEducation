import {
    AppShell,
    Navbar,
    Header,
    Footer,
    Aside,
    Text,
    MediaQuery,
    useMantineTheme,
    Breadcrumbs,
    Anchor,
    Group,
    Menu,
    Button
  } from '@mantine/core';
  import { Brand } from '../_brand';
  import { UserButton } from '../_user';
  import { MainLinks } from '../_mainLinks';
import useAuth from '../../Providers/auth.provider';
import { User } from '../../Models/user';
import { IconExternalLink } from '@tabler/icons-react';
import { getBooks }  from '../../API/books.api';
  
  function convertURL(url : string) {
    const path = url.split('/').filter(Boolean);
    let base = '';
    return path.map((p, i) => {
      base += `/${p}`;
      return { title: p, href: base };
    }).map((item, index) => (
      <Anchor href={item.href} key={index}>
        {item.title}
      </Anchor>
    ));
  }
  
  interface Props {
    children: React.ReactNode;
    history: History;
    guardData?: object;
  }
  
function dashboard(props : any) {
    const { children, history, location, guardData } : any = props;
    const { user, logout }: Partial<{ user: User, logout: () => void }> = useAuth();
    
   

    const bcItems = convertURL(location.pathname)
    const theme = useMantineTheme();
    return (
  
      //<BrowserRouter>
        <AppShell
          styles={{
            main: {
              background: theme.colorScheme === 'dark' ? theme.colors.dark[8] : theme.colors.gray[0],
            },
          }}
          navbarOffsetBreakpoint="sm"
          asideOffsetBreakpoint="sm"
          navbar={
            <Navbar p="md" hiddenBreakpoint="sm" width={{ sm: 200, lg: 300 }}>
              <MainLinks />
            </Navbar>
          }
          aside={
            <MediaQuery smallerThan="sm" styles={{ display: 'none' }}>
              <Aside zIndex={1} p="md" hiddenBreakpoint="sm" width={{ sm: 200, lg: 300 }}>
                <Text>Application sidebar</Text>
              </Aside>
            </MediaQuery>
          }
          footer={
            <Footer height={60} p="md">
              Application footer
            </Footer>
          }
          header={
            <Header height={{ base: 50, md: 70 }} p="md">
              <div style={{ justifyContent: 'space-between', display: 'flex', alignItems: 'center', height: '100%' }}>
                <Brand />
                <Breadcrumbs separator="→">{bcItems}</Breadcrumbs>
               
                    <Group>
                    {user &&  <Menu width={200} shadow="md"  withArrow zIndex={10}>
                      <Menu.Target>
                        <UserButton
                          image="https://images.unsplash.com/photo-1508214751196-bcfd4ca60f91?ixid=MXwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHw%3D&ixlib=rb-1.2.1&auto=format&fit=crop&w=255&q=80"
                          name={user.UserName} 
                          email={user.Email}
                        /> 
                      </Menu.Target>
                      <Menu.Dropdown >
                      <Menu.Item
                        icon={<IconExternalLink size={14} />}
                        onClick={logout}
                        component="a" 
                        href="/"
                      >
                        Logout
                      </Menu.Item>
                    </Menu.Dropdown>
                    </Menu> ||  <div style={{ width: 214 }}></div>}
                    </Group>
                    
              {/* <NavLink label="Change Count" component="a" href="/home" target="_blank" />
              <NavLink label="API Documentation" component="a" href="/home" target="_blank" /> */}
              </div>
            </Header>
          }>
          {children}
        </AppShell>
      //</BrowserRouter>
    );
  }

  export default dashboard;