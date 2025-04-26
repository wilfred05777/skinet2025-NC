/* superset of javascript */

// let message: string | number = "Hello";
let message = "Hello"; // inferred type
// let isComplete: boolean = false;
let isComplete = false;

// message = 42; // This is allowed because message can be a string or a number

// isComplete = 1;


/* interface Todo {
  {
    id: number;
    title: string;
    completed: boolean;
  }
  they are equivalent to type 'Todo = {' below
*/


type Todo = {
  id: number;
  title: string;
  completed: boolean;
}


let todos: Todo[] = [];

function addTodo(title: string): Todo {
  const newTodo: Todo ={
    id: todos.length + 1,
    title: title,
    completed: false
  }
  todos.push(newTodo);
  return newTodo;
}


/* if function does not return anything we return 'void' */
function toggleTodo(id: number): void {
  const todo  = todos.find(todo => todo.id === id);
  if(todo){
    todo.completed = !todo.completed;
  }

}


addTodo("Learn TypeScript");
addTodo("Publish app");
toggleTodo(1);


console.log(todos);

// ` to compile it  npx tsc src/demo.ts `
