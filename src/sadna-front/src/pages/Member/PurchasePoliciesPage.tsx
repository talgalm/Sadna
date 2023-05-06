
import { Container, ListGroup } from 'react-bootstrap';
import React, { useState,useEffect } from 'react';
import { Button, Modal, Form , Row, Col } from 'react-bootstrap';
import { Condition } from '../../components/Condition';
import Exit from "../Exit.tsx";
import { useLocation } from 'react-router-dom';
import { handleGetAllPurchaseConditions, handleGetPurchaseCondition } from '../../actions/MemberActions.tsx';
import { PurcahseCondition , Item , Store} from '../../models/Shop.tsx';


function PurchasePoliciesPage(props) {
  const location = useLocation();
  const { userId, storeId } = location.state;
  const items = [
    { id: 1 ,  entity:'Item' , entityID:11 , type:'min quantity' , value:0,},
    { id: 2 ,  entity:'Item' , entityID:12 , type:'max quantity' , value:1 ,
     op:'Conditioning' , entityRes:'Item' , entityIDRes:14 , typeRes:'max quantity' , valueRes:0 , opCond:-1},
     { id: 3 ,  entity:'Item' , entityID:14 , type:'max quantity' , value:10,op:'AND' , opCond:1},
  ];
  const [allItems, setAllItems] = useState(items);

  const [policysList, setPolicyList] = useState<PurcahseCondition[]>([]);


  const [showModal, setShowModal] = useState(false);
  const [entity, setEntity] = useState('');
  const [EntityChoice, setEntityChoice] = useState('');
  const [WhichCond, setWhichCond] = useState('');
  const [condValue, setCondValue] = useState('');
  const [entity2, setEntity2] = useState('');
  const [EntityChoice2, setEntityChoice2] = useState('');
  const [WhichCond2, setWhichCond2] = useState('');
  const [condValue2, setCondValue2] = useState('');
  const [selectedOption, setSelectedOption] = useState('');

  const [showTextbox, setShowTextbox] = useState(false);

  

  const handleButtonClick = () => {
    setShowTextbox(!showTextbox);
  }
  
  const handleOptionChange = (event) => {
    setSelectedOption(event.target.value);
  }

  const openModal = () => {
    setShowModal(true);
  };

  const closeModal = () => {
    setShowModal(false);
  };


  const GetPurcahsePolicys =()=>{
    handleGetAllPurchaseConditions(storeId).then(
      value => {
        setPolicyList(value as PurcahseCondition[]);
      })
      .catch(error => alert(error));
  }

  useEffect(() => {
    GetPurcahsePolicys();
 }, [])

  const handleSubmit = () => {
  };
return (
    <div className="container mt-5">
            <Exit id={props.id}/>
        <ListGroup>
          <h5>Create New Store Policy</h5>
         </ListGroup>
         <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
      <Button variant="dark"  onClick={openModal} style={{ marginLeft: '10px' }}>
        Add Condition
      </Button>
       <span>                                               </span>
      <Button variant="dark" onClick={handleButtonClick} style={{ marginLeft: '10px' }}>
        Remove Condition
      </Button>
      {showTextbox && (
        <div>
          <label>Enter condition's ID:</label>
          <input type="text" />
        </div>
      )}
      </div>
      <p></p>

      {/* <Row className="mt-3">
          {allItems.map((item) => (
            <Col sm={8} md={5} lg={4} xl={3} key={item.id} className="mt-3">
              <Condition {...item}></Condition>
            </Col>
            ))}
        </Row> */}
                <Row className="mt-3">
          {policysList.length===0? (<div>  No Items </div>): (policysList.map((cond) => (
            // <div key={item.name}>{item.name} </div>
            <Col  key={cond.condID} className="mt-3">
              <Condition { ...cond}></Condition>
            </Col>
            )))}
        </Row>
        



      <Modal show={showModal} onHide={closeModal}>
        <Modal.Header closeButton>
          <Modal.Title>Add Condition</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          <Form>
          <Form.Group>
              <Form.Label>Entity</Form.Label>
              <Form.Control
                as="select"
                value={EntityChoice}
                onChange={(e) => setEntityChoice(e.target.value)}
              >
                <option>Store</option>
                <option>Shooping cart</option>
                <option>Category</option>
                <option>Item</option>
              </Form.Control>
            </Form.Group>
            <Form.Group>
              <Form.Label>Entity ID</Form.Label>
              <Form.Control
                type="ID"
                placeholder="Enter entity's ID"
                value={entity}
                onChange={(e) => setEntity(e.target.value)}
              />
            </Form.Group>
            <Form.Group>
              <Form.Label>Which Condition</Form.Label>
              <Form.Control
                as="select"
                value={WhichCond}
                onChange={(e) => setWhichCond(e.target.value)}
              >
                <option>Select type</option>
                <option>Min Value</option>
                <option>Max Value</option>
                <option>Min Quantity</option>
                <option>Max Quantity</option>
                <option>Time Condition</option>
              </Form.Control>
            </Form.Group>
            <Form.Group>
              <Form.Label>Condition Value</Form.Label>
              <Form.Control
                type="value"
                placeholder="Enter value"
                value={condValue}
                onChange={(e) => setCondValue(e.target.value)}
              />
            </Form.Group>
            </Form>
            <Form.Group>
            <Form.Label>Operator</Form.Label>
            <Form.Control as="select" onChange={handleOptionChange}>
            <option value="option">Select an option</option>
            <option value="option1">AND</option>
            <option value="option2">OR</option>
            <option value="option3">Conditional</option>
          </Form.Control>
          </Form.Group>
          {selectedOption === 'option3' && (
            <div>
                    <Form.Group>
                      <Form.Label>Entity</Form.Label>
                      <Form.Control
                        as="select"
                        value={EntityChoice2}
                        onChange={(e) => setEntityChoice2(e.target.value)} >
                        <option>Store</option>
                        <option>Shooping cart</option>
                        <option>Category</option>
                        <option>Item</option>
                      </Form.Control>
                    </Form.Group>
                    <Form.Group>
                      <Form.Label>Entity ID</Form.Label>
                      <Form.Control
                        type="ID"
                        placeholder="Enter entity's ID"
                        value={entity2}
                        onChange={(e) => setEntity2(e.target.value)}
                      />
                    </Form.Group>
                    <Form.Group>
                      <Form.Label>Which Condition</Form.Label>
                      <Form.Control
                        as="select"
                        value={WhichCond2}
                        onChange={(e) => setWhichCond2(e.target.value)}
                      >
                        <option>Select type</option>
                        <option>Min Value</option>
                        <option>Max Value</option>
                        <option>Min Quantity</option>
                        <option>Max Quantity</option>
                        <option>Time Condition</option>
                      </Form.Control>
                    </Form.Group>
                    <Form.Group>
                      <Form.Label>Condition Value</Form.Label>
                      <Form.Control
                        type="value"
                        placeholder="Enter value"
                        value={condValue2}
                        onChange={(e) => setCondValue2(e.target.value)}
                      />
                    </Form.Group>
            </div>
          )}
        </Modal.Body>
        <Modal.Footer>
          <Button variant="secondary" onClick={closeModal}>
            Close
          </Button>
          <Button variant="primary" onClick={handleSubmit}>
            Create
          </Button>
        </Modal.Footer>
      </Modal>
    </div>
  );
}
export default PurchasePoliciesPage;