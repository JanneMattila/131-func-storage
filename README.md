# 131 - Azure Functions and sharing files via storage

Example implementation about Valet Key pattern:

![image](https://user-images.githubusercontent.com/2357647/92996451-560c0200-f514-11ea-84eb-022f929ffa7d.png)

Upload file via API:

```bash
### Upload excel file
POST {{func}}/api/storage HTTP/1.1
Content-type: application/vnd.openxmlformats-officedocument.spreadsheetml.sheet
Filename: CorporateFinance.xlsx
Validity: 600

< ./doc/example.xlsx
```

You'll get response with link to the uploaded file appended with sas token:

```json
{
  "uri": "http://127.0.0.1:10000/devstoreaccount1/files/CorporateFinance.xlsx?sv=2019-12-12&se=2020-09-12T13%3A21%3A57Z&sr=b&sp=r&sig=qdzylLTO8JkhtgDn3Soc1TaP0OLQ6vUQ4N0ZetDKLOM%3D",
  "expiresOn": "2020-09-12T13:21:57.4141973+00:00"
}
```

## Links

Read more about [Valet Key pattern](https://docs.microsoft.com/en-us/azure/architecture/patterns/valet-key) from
Azure Design patterns.