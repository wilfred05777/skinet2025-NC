/* superset of javascript */
// let message: string | number = "Hello";
var message = "Hello"; // inferred type
// let isComplete: boolean = false;
var isComplete = false;
var todos = [];
function addTodo(title) {
    var newTodo = {
        id: todos.length + 1,
        title: title,
        completed: false
    };
    todos.push(newTodo);
    return newTodo;
}
/* if function does not return anything we return 'void' */
function toggleTodo(id) {
    var todo = todos.find(function (todo) { return todo.id === id; });
    if (todo) {
        todo.completed = !todo.completed;
    }
}
addTodo("Learn TypeScript");
addTodo("Publish app");
toggleTodo(1);
console.log(todos);
// ` to compile it  npx tsc src/demo.ts `
