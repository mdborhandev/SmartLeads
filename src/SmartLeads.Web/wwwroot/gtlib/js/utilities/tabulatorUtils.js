let gt_table;
let gt_childTables = []; 

function buildTabulatorLayout(rootSelector, tableId, opts) {
    // Main container with some spacing
    let design = `<div class="card"><div class="card-body">`;

    // Header section with search and buttons
    design += `<div class="d-flex justify-content-between align-items-center mb-3">`;
    
    // Search input on the left
    design += `<div class="flex-grow-1 me-3 w-50" >
                   <input type="text" id="tabulator-search" class="form-control" placeholder="Search..."/>
               </div>`;

    // Buttons on the right
    design += `<div class="btn-group">
                   <div class="dropdown">
                       <button class="btn btn-sm btn-primary" type="button" id="column-visibility-button" data-bs-toggle="dropdown" aria-expanded="false">
                         <i class="icon-base bx bx-slider"></i>
                       </button>
                       <ul class="dropdown-menu" id="column-visibility-menu" aria-labelledby="column-visibility-button">
                           <!-- Column items will be populated by JavaScript -->
                       </ul>
                   </div>
                   <div class="dropdown ms-2">
                       <button class="btn btn-sm btn-primary" type="button" id="grouping-button" data-bs-toggle="dropdown" aria-expanded="false">
                          <i class="icon-base bx bx-list-ul"></i>
                       </button>
                       <ul class="dropdown-menu" id="grouping-menu" aria-labelledby="grouping-button">
                           <!-- Grouping items will be populated by JavaScript -->
                       </ul>
                   </div>
                    <div class="dropdown ms-2">
                       <button class="btn btn-sm btn-primary" type="button" id="grouping-button" data-bs-toggle="dropdown" aria-expanded="false">
                          <i class="icon-base bx bx-printer"></i>
                       </button>
                       <ul class="dropdown-menu" id="grouping-menu" aria-labelledby="grouping-button">
                         <li><a id="export-xlsx" class="cursor-pointer"><i class="icon-base bx bx-bxs-file-pdf"></i>Export XLSX</a></li>
                         <li><a id="export-csv" class="cursor-pointer"><i class="icon-base bx bx-bxs-file-doc"></i>CSV</a></li>
                         <li><a id="export-pdf" class="cursor-pointer"><i class="icon-base bx bx-bxs-file-export"></i>PDF</a></li>
                       </ul>
                   </div>
                   
               </div>`;
    design += `</div>`;
    // -- End header

    // Table container
    design += `<div class="row mt-2"><div id="${tableId}" style="height: 100%;"></div></div>`;

    // Footer buttons
    design += `<div class="d-flex justify-content-start align-items-center mt-3">
                   <button id="add-row" class="btn btn-sm btn-primary">Add Row</button>
                   <button id="clear-all" class="btn btn-sm btn-danger ms-2">Clear Row</button>
               </div>`;

    design += `</div></div>`; // End card

    var root = document.querySelector(rootSelector);
    if (root) {
        root.innerHTML = design;
        return '#' + tableId;
    }
    return null;
}


// initialize tabulator
function initTabulator(selector, layoutopts, tabulatorConfig) {
    var tableid = buildTabulatorLayout(selector, 'Gt-tabulator', layoutopts);
    if (!tableid) {
        console.error("Failed to build tabulator layout.");
        return;
    }

    gt_table = new Tabulator(tableid, tabulatorConfig);

    gt_table.on("tableBuilt", function(){
        populateColumnVisibilityMenu(gt_table);
        populateGroupingMenu(gt_table);
        setupEventListeners(gt_table);
    });

    return gt_table;
}



function populateColumnVisibilityMenu(table) {
    const menu = document.getElementById('column-visibility-menu');
    if (!menu) return;

    menu.innerHTML = '';
    const columns = table.getColumns();

    columns.forEach(column => {
        const definition = column.getDefinition();
        if (!definition.title || definition.field === "rowSelection") return;

        const field = definition.field;

        const li = document.createElement('li');
        li.classList.add('dropdown-item', 'd-flex', 'align-items-center');

        const checkbox = document.createElement('input');
        checkbox.type = 'checkbox';
        checkbox.classList.add('form-check-input', 'me-2');
        checkbox.checked = column.isVisible();
        checkbox.id = `col-vis-${field}`;

        const label = document.createElement('label');
        label.classList.add('form-check-label');
        label.htmlFor = checkbox.id;
        label.textContent = definition.title;

        checkbox.addEventListener('change', function () {

            // ? MAIN TABLE
            this.checked ? column.show() : column.hide();

            // ? CHILD TABLES (if exists)
            if (Array.isArray(gt_childTables) && gt_childTables.length > 0) {
                gt_childTables.forEach(childTable => {
                    const childCol = childTable.getColumn(field);
                    if (childCol) {
                        this.checked ? childCol.show() : childCol.hide();
                    }
                });
            }
        });

        li.appendChild(checkbox);
        li.appendChild(label);
        menu.appendChild(li);
    });
}




//function populateColumnVisibilityMenu(table) {
//    const menu = document.getElementById('column-visibility-menu');
//    if (!menu) return;

//    menu.innerHTML = '';
//    const columns = table.getColumns();

//    columns.forEach(column => {
//        const definition = column.getDefinition();
//        if (!definition.title || definition.field === "rowSelection") return;

//        const li = document.createElement('li');
//        li.classList.add('dropdown-item', 'd-flex', 'align-items-center');

//        const checkbox = document.createElement('input');
//        checkbox.type = 'checkbox';
//        checkbox.classList.add('form-check-input', 'me-2');
//        checkbox.checked = column.isVisible();
//        checkbox.id = `col-vis-${column.getField()}`;

//        const label = document.createElement('label');
//        label.classList.add('form-check-label');
//        label.htmlFor = checkbox.id;
//        label.textContent = definition.title;

//        // ?? Single toggle logic
//        checkbox.addEventListener('change', function () {
//            if (this.checked) {
//                column.show();
//            } else {
//                column.hide();
//            }
//        });

//        li.appendChild(checkbox);
//        li.appendChild(label);
//        menu.appendChild(li);
//    });
//}


function populateGroupingMenu(table) {
    const menu = document.getElementById('grouping-menu');
    if (!menu) return;
    menu.innerHTML = '';
    const columns = table.getColumns();

    const clearLi = document.createElement('li');
    const clearLink = document.createElement('a');
    clearLink.classList.add('dropdown-item');
    clearLink.href = '#';
    clearLink.textContent = 'Clear Grouping';
    clearLink.addEventListener('click', (e) => {
        e.preventDefault();
        table.setGroupBy(false);
    });
    clearLi.appendChild(clearLink);
    menu.appendChild(clearLi);

    columns.forEach(column => {
        const field = column.getField();
        if (!field || column.getDefinition().field === "rowSelection") return;

        const li = document.createElement('li');
        const link = document.createElement('a');
        link.classList.add('dropdown-item');
        link.href = '#';
        link.textContent = column.getDefinition().title || field;
        link.addEventListener('click', (e) => {
            e.preventDefault();
            table.setGroupBy(field);
        });

        li.appendChild(link);
        menu.appendChild(li);
    });
}

function setupEventListeners(table) {
    // Global search
    const searchInput = document.getElementById('tabulator-search');
    if (searchInput) {
        searchInput.addEventListener('keyup', function(){
            const searchTerm = searchInput.value;
            // A custom filter function that searches all fields
            table.setFilter(function(data) {
                if (!searchTerm) {
                    return true; //show all rows if search term is empty
                }
                for (const key in data) {
                    // Do not search in the children array itself, Tabulator handles tree filtering
                    if (key === '_children') {
                        continue;
                    }
                    if (data[key] !== null && data[key] !== undefined) {
                        if (String(data[key]).toLowerCase().includes(searchTerm.toLowerCase())) {
                            return true; // Match found
                        }
                    }
                }
                return false; // No match found
            });
        });
    }

    // Export buttons
    document.getElementById('export-xlsx')?.addEventListener('click', () => table.download("xlsx", "data.xlsx", { sheetName: "My Data" }));
    document.getElementById('export-csv')?.addEventListener('click', () => table.download("csv", "data.csv"));
    document.getElementById('export-pdf')?.addEventListener('click', () => table.download("pdf", "data.pdf", {
        orientation:"landscape", 
        autoTable:{
            theme: 'grid'
        }
    }));

    // Add Row button
    document.getElementById('add-row')?.addEventListener('click', () => {
        const newId = Math.max(...table.getData().map(r => r.id), 0) + 1;
        const newRow = {
            id: newId,
            name: "New Person",
            empCode: (1000 + newId).toString(),
            employeeType: "Permanent",
            department: "IT",
            jobTitle: "New Role",
            age: 30,
            gender: "Male",
            height: 170,
            dob: "01/01/1990"
        };
        table.addRow(newRow, true); // Add row to the top of the table
    });

    // Clear All button
    document.getElementById('clear-all')?.addEventListener('click', () => {
        table.clearData();
    });
}

function addRowToTabulator(table, rowData) {
    if (table && rowData) {
        table.addRow(rowData)
            .catch((error) => console.error("Error adding row:", error));
    } else {
        console.error("Invalid table or rowData!");
    }
}

function addRowsToTabulator(table, rowsData) {
    if (table && Array.isArray(rowsData) && rowsData.length > 0) {
        table.addData(rowsData)
            .catch((error) => console.error("Error adding rows:", error));
    } else {
        console.error("Invalid table or rowsData!");
    }
}

function setDataToTabulator(table, newData) {
    if (table && Array.isArray(newData)) {
        table.setData(newData)
            .catch((error) => console.error("Error setting data:", error));
    } else {
        console.error("Invalid table or newData!");
    }
}
function setUrlForRemotePagination(table, url) {
    if (table && url) {
        table.setData(url)
            .catch((error) => console.error("Error setting url:", error));
    } else {
        console.error("Invalid table or url!");
    }
}