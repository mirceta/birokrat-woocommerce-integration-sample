WARNING - THE CURRENT PRODUCTION BRANCH IS NOT MASTER, BUT **production**!

YOU NEED TO MERGE MASTER INTO PRODUCTION, THEN BACK TO MASTER!!!!


### Production

- VMBIROBAZURE2: branch production-backup2 -> oldest version
- VMBIROBAZURE2: branch nove-integracije -> new version


### Pravila integracij

#### Podatkovna baza

- Vsakemu produktu pripada natanko ena SKU koda.



# Assigned tasks

Is the plugin that enables distributing the integrations application workload across multiple servers.

Here is the high level map of how this system is supposed to work:
![assigned_tasks_highlevel_services_communication_map](https://github.com/mirceta/bironext-woocommerce-integration/assets/7331601/2ebd4114-b5a1-4de2-a763-e3495032026c)


## Assigned task tutorial

### What is an assigned task?

An assigned task is a task that:

- is executed on a worker server
- is associated with an integration object and optionally a version of it (integrationId, versionId)
- Examples are: production loop execution for orders, for products, reentering a single order again - out of order, validating products on the website, etc...

### Assigned task constitution

Each AssignedTask is partitioned into 3 parts: A general part, the frontend part and the backend part.

- The **general part** is relevant both for matters of the frontend and the backend. It's comprised of:
	- the integration object: Each assigned task is associated with exactly one integration object.
	- A form - these are additional parameters needed to execute the task, that are not innately defined in the integration object.
	- A status - which is individual for each task and has no innate meaning. It is a signifier only to be used within the execution of the task and not outside of it.
	- A set of **dependencies** for the task. The task may produce results or be influenced by any set of external dependencies that are normally shared between the frontend and the backend. Namely for example - say a task generates pdf documents. This means that the frontend will need to be able to view these pdf documents, while the backend will need to be able to write those pdf documents.

- The **frontend part** is all of the matters regarding the AssignedTask that are associated with the user's ability to control the task through a user interface. These are matters like the creation of the task, viewing of the task state, stopping the task or altering the nature of the task's execution. This part is comprised of:
	- **Main**:
	- The frontend program: Which is written in the **birolang** language as defines the lifecycle hooks (for now only onCreate()) and actions available to the user in order to influence the AssignedTask.
	- **Dependencides**:
		- Here the dependencies are defined in the root of the program and injected into the assigned task frontend model,
		- The functionality associated with the dependencies is exposed through the frontend **extensions** of the assigned task.

- The **backend part** is all of the matters regarding the AssignedTask that are associated with the execution of the task on a designated worker server. This part is comprised of:
	- **Main**:
		- the definition of the workload of the task - ITests implementation.
		- Modifying parameters such as the bironext address or webshop address and credentials. This is done through interception of the integration object building process.
	- **Dependencies**
		  - Interception and working with the dependencies is also done through the interception of both the integration object building process and interception of the workload execution process.


### Frontend connection

The frontend connection for a specific new AssignedTask **X** is defined by overriding the following interfaces:

```
public interface IAssignedTaskFrontendFactory {
	Task BeforeCreate(FDllInfo dinfo, AssignedTasksCreateRequest request);
	Task<ITaskBuilder> Create(int integrationId);
	List<IAssignedTaskExtension> GetExtensions();
}
public interface ITaskBuilder
{
	Task<AssignedTaskFrontendModel> PrepareForFrontend(AssignedTask task);
}
public class FormElement
{
	public string name { get; set; }
	public string type { get; set; }
	public string label { get; set; }
	public string value { get; set; }
}
public class AssignedTaskFrontendModel
{
	public string Status { get; set; }
	public List<FormElement> Form { get; set; }
	public Dictionary<string, object> Data { get; set; }
	public string Program { get; set; }
}
public class AssignedTasksCreateRequest
{
	public string IntegrationId { get; set; }
	public string VersionId { get; set; }
	public List<FormElement> FormElements { get; set; }
}
```
![assigned_tasks_frontend_highlevel](https://github.com/mirceta/bironext-woocommerce-integration/assets/7331601/532b8bf5-4965-449a-bb39-1f6d3dbc3bce)

