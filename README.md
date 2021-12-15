![logo](https://raw.githubusercontent.com/micahmo/RDPoverSSH/main/RDPoverSSH/Images/logo.png)


# RDPoverSSH

A peer-to-peer Windows Remote Desktop solution using SSH.

## About

RDPoverSSH uses SSH tunnels with certificate authentication to map a port on the local machine to any port on the remote machine. It is specifically designed for remote control with Microsoft's Remote Desktop Protocol, although any port mapping may be configured.

It is especially useful in an environment where the target machine is behind a NAT router or firewall that is under someone else's control. Using reverse SSH tunnels, the target machine can initiate the connection outwards. Once the tunnel is open and a port is mapped to the target machine, it is available to receive connections without any port forwarding required on the target side.

For some background on how reverse SSH tunnels work, check out the several great answers on this StackOverflow question. https://unix.stackexchange.com/questions/46235/how-does-reverse-ssh-tunneling-work

> Note that port forwarding is required on at least one side of the connection. RDPoverSSH will not work if no port forwarding is possible.

## Download

Download the latest release [here](https://github.com/micahmo/RDPoverSSH/releases/latest).

## How to Use

Read the scenario descriptions to determine which is most applicable. Note that the following is true in both cases:
 - Port forwarding must be configured on one end or the other.
 - It is recommended to begin configuration on whichever machine will be the SSH server because...
   1. You will have to copy the SSH server private key to the SSH client.
   2. In case of a reverse tunnel, you will have copy the server local tunnel port to the client.

### Scenario 1 (Normal Tunnel)

> Use this mode if you want to connect from Machine A to B, and you have permissions configure port forwarding on Machine B's network.

**Machine B (Connection Target, SSH Server)**

Start by configuring your router to forward a public port to port 22 on Machine B. This process will look very different depending on your router and is beyond the scope of this document. The following examples will use port 443 (see [Public Port Selection](https://github.com/micahmo/RDPoverSSH#public-port-selection)), but any public port may be used.

Configure both the Connection and Tunnel to point to the local computer. Wait for the tunnel status to become good, as indicated by a green checkmark. (If there is any error with the tunnel, it will be indicated by a red X. Hover to see the error details.)

![image](https://user-images.githubusercontent.com/7417301/146197233-1aa50e77-00d9-420c-8042-5bc26feb0007.png)

After configuring Machine B, we will need to obtain two values to use on Machine A.
 - The SSH server private key, which can be obtained by pressing this icon: ![image](https://user-images.githubusercontent.com/7417301/146196089-29bf92b0-f653-4db2-a211-be6c3f1ee833.png)
 - The machine's public IP address. If you don't already know the public IP (or have a domain name), use this icon to copy the public IP to the clipboard: ![image](https://user-images.githubusercontent.com/7417301/146196126-e12faafd-435c-4336-a6ed-f3bd31eac1ec.png)

**Machine A (Connection Source, SSH Client)**

On Machine A, configure both the Connection and Tunnel to point to the remote computer.
* In the "Destination Port" field, choose the port that you want to connect to on Machine B. There are a few default ports, but any port may be selected. We will use 3389 for RDP.
* In the "Tunnel IP address or name" field, enter the public IP address of Machine B as obtained previously.
* In the "Tunnel Port" field, enter the public port that was previously mapped to Machine B's port 22.
* Finally, click the following icon and enter the SSH server private key from Machine B as obtained previously. ![image](https://user-images.githubusercontent.com/7417301/146196089-29bf92b0-f653-4db2-a211-be6c3f1ee833.png)

![image](https://user-images.githubusercontent.com/7417301/146196749-8dbd3b25-7868-40e1-abed-f6472165b1db.png)

Now you are ready to connect! Press the Connect button to initiate an RDP session.

If there are any tunnel errors, they will be indicated by a red X. Click the X to obtain further details about the tunnel error. These will be standard SSH errors, so searching for the error text should give a good clue as to the cause.

### Scenario 2 (Reverse Tunnel)

> Use this mode if you want to connect from Machine A to B, but you only have permissions to configure port forwarding on Machine A (for example, if Machine B is behind a corporate firewall).

This mode uses an SSH reverse tunnel, in which Machine B initiates the connection to Machine A, after which connections can travel over a port mapped from Machine A to another port on Machine B.

**Machine A (Connection Source, SSH Server)**

Start by configuring your router to forward a public port to port 22 on Machine A. This process will look very different depending on your router and is beyond the scope of this document. The following examples will use port 443 (see [Public Port Selection](https://github.com/micahmo/RDPoverSSH#public-port-selection)), but any public port may be used.

On Machine A, configure the Connection to point to the remote computer and the Tunnel to point to the local computer. Again, we will choose 3389 (RDP) as the Destination Port.

> A random, available local port will selected for the reverse tunnel (in this example, 52885). It is not recommended to change this.

> Until we have configured Machine B, it is normal to see a Warning icon. This indicates that the SSH server has been started, but no machine has established a reverse tunnel yet.

![image](https://user-images.githubusercontent.com/7417301/146198095-99aed5ee-d4a7-4061-a450-a3495aa95044.png)

As in scenario 1, we will need two values from Machine A.
 - The SSH server private key, which can be obtained by pressing this icon: ![image](https://user-images.githubusercontent.com/7417301/146196089-29bf92b0-f653-4db2-a211-be6c3f1ee833.png)
 - The machine's public IP address. If you don't already know the public IP (or have a domain name), use this icon to copy the public IP to the clipboard: ![image](https://user-images.githubusercontent.com/7417301/146196126-e12faafd-435c-4336-a6ed-f3bd31eac1ec.png)

**Machine B (Connection Target, SSH Client)**

On Machine B, configure the Connection to point to the local computer and the Tunnel to point to the remote computer.
* In the "Destination Port" field, choose the port that you want the remote computer to connect to on the local computer. We will use 3389 (RDP).
* In the "Tunnel IP address or name" field, enter the public IP address of Machine A as obtained previously.
* In the "Tunnel Port" field, enter the public port that was previously mapped to Machine A's port 22.
* In the next field, enter the tunnel local port that was automatically selected on Machine A (in this example, 52885).
* Finally, click the following icon and enter the SSH server private key from Machine A as obtained previously. ![image](https://user-images.githubusercontent.com/7417301/146196089-29bf92b0-f653-4db2-a211-be6c3f1ee833.png)

![image](https://user-images.githubusercontent.com/7417301/146200622-83c29e0f-2ff8-4a31-9f74-3d35e67e6cc4.png)


If the tunnel is established successfully, there should be a green checkmark on Machine B. If so, the tunnel status on Machine A should also have updated from the yellow warning sign to a green checkmark.

![image](https://user-images.githubusercontent.com/7417301/146201134-5055c165-fb09-4b64-9844-8405508c907f.png)

If there are any tunnel errors, they will be indicated by a red X. Click the X to obtain further details about the tunnel error. These will be standard SSH errors, so searching for the error text should give a good clue as to the cause.

## Misc

Once you have finished configuring your connections, you can do some of the following:
 - Enter connection names. This allows you to use the filtering feature to see a subset of your connections.
 - Collapse the settings section. This condenses the UI and shows only the most important connection information at a glance.
 - Create RDP profiles. If the Connection Port is 3389, the Connect button will show a dropdown from which you can create custom profiles for the connection.

![image](https://user-images.githubusercontent.com/7417301/146201625-70ee19d6-acfa-46a3-8264-5f75b9fa7b45.png)

> For connection ports other than 3389 (RDP), the default behvior of the Connect button is to open the browser. However, the port mapping may be utilized by other applications.

> Note that the app does not need to stay open in order to maintain the connections.

### Public Port Selection

In the examples above, public port 443 was forwarded to the SSH server machine's port 22. This is a useful practice for a few reasons.
1. Many port scanners hammer port 22. While using 443 as the public SSH port doesn't make your SSH server more secure per se (that's what the public/private keys are for), using a non-standard public port will reduce the amount of unwanted traffic.
2. Using a reverse tunnel to initiate an SSH connection from inside a corporate firewall is often not sufficient. Corporate firewalls may also block traffic on port 22, even outgoing. But, without deep packet inspection, they cannot block a standard HTTP port like 443. Using that as the SSH endpoint raises the chances that it will be able to get out of a corporate firewall.

That said, you may already be hosting other web services on your public port 443, which you may already be forwarding to an nginx reverse proxy, for example. In that case, how can you also use it for SSH? In comes a useful tool called [sslh](https://www.rutschle.net/tech/sslh/README.html). Their log line describes this exact scenario.

> A typical use case is to allow serving several services on port 443 (e.g. to connect to SSH from inside a corporate firewall, which almost never block port 443) while still serving HTTPS on that port.
You can configure it to handle all requests on 443. It will do packet inspection and route HTTP packets to port 443 your web service while routing SSH packets to port 22 of your SSH server.

It sits behind your port 443 and receives all traffic on that port. It then routes based on packet type. For HTTP(S) packets, it can route to your web server (e.g., nginx) and for SSH packets, it can route to your SSH server. Configuring sslh is beyond the scope of this document.

## Requirements

Requires Windows 10 or later.

# Attribution
[Icon](https://www.flaticon.com/premium-icon/data-transfer_2985993) made by [Prosymbols Premium](https://www.flaticon.com/authors/prosymbols-premium) from [www.flaticon.com](https://www.flaticon.com/).