# Postman assets

Import both files into Postman:

1. `COINK.postman_collection.json`
2. `COINK.local.postman_environment.json`

Select **COINK Local** and start the default Docker Compose stack. Run requests
individually or run the collection in folder order. **Create user** stores the
returned identifier in environment variable `userId`; get, update, and delete
reuse it. The environment contains only local URLs, fixed public catalog IDs,
and an empty runtime identifier—no credential or secret.

The sample uses Colombia (`170`), Antioquia (`5`), and Medellín (`5001`). Change
variables after querying geography if testing a different hierarchy. Re-running
Create is allowed because phone uniqueness is not an assessment rule.
